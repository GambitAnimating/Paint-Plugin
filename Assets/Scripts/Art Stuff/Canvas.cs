using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Canvas : MonoBehaviour
{
    [Serializable]
    public struct BrushData
    {
        public Texture2D ActiveTexture;

        public Texture2D SrcTexture;
        
        public Color Color;
        public NativeArray<UINTColor32> Data;
        public NativeArray<uint> MaskData;
    }
    
    [SerializeField] private BrushData CurrentBrush;

    public GameObject RawImage;
   
    private readonly int _chunkSize = 128;
   
    private readonly Dictionary<string, Dictionary<Vector2Int, CanvasChunk>> chunksDict = new();
    private readonly Dictionary<Vector2Int, CanvasChunk> _tempChunkDict = new();
    
    private UnsafeList<UnsafeList<UINTColor32>> _tempChunkPixels;
    private UnsafeList<UnsafeList<UINTColor32>> _tempChunkMask;
    
    private UnsafeList<UnsafeList<UINTColor32>> _realChunkPixels;

    private UnsafeList<UnsafeList<UINTColor32>> _brushPixels;
    private UnsafeList<UnsafeList<uint>> _brushMask;

    private List<Texture2D> _texturesToUpdate = new();
    
    private JobHandle paintModificationHandle; // 1
    private BoringJobFor paintModificationJob;
    
    JobHandle paintToRealModificationHandle; // 1
    PaintToRealJob paintToRealJob;

    public void FinishDraw()
    {
        PaintTempToReal();
        ResetTemp();
    }

    public void DrawToCanvas(Vector2Int position)
    {
        PaintArtChunk(position.x, position.y);
    }

    public void SetupBrushData(Texture2D brush)
    {
        CurrentBrush.ActiveTexture = brush;
        if (CurrentBrush.ActiveTexture != null)
        {
            CurrentBrush.Data = CurrentBrush.ActiveTexture.GetRawTextureData<UINTColor32>();
            //CurrentBrush.MaskData = this.CurrentBrush.ActiveTexture.GetRawTextureData<byte>();
            CurrentBrush.MaskData = new NativeArray<uint>(CurrentBrush.Data.Length, Allocator.Temp);
            
            UINTColor32 white = Color.white;
            UINTColor32 black = Color.black;

            for (int i = 0; i < CurrentBrush.Data.Length; i++)
            {

                if (CurrentBrush.Data[i].a >= 25)
                {

                    CurrentBrush.MaskData[i] = black.Rgba;
                }
                else
                {
                    CurrentBrush.MaskData[i] = white.Rgba;
                }
            }
        }
    }
    
    private void LateUpdate()
    {
       UpdateTextures();
    }

    private void UpdateTextures()
    {
        if (_texturesToUpdate.Count > 0)
        {
            if (paintModificationHandle.IsCompleted != true)
                paintModificationHandle.Complete();
            
            if (paintToRealModificationHandle.IsCompleted != true)
                paintToRealModificationHandle.Complete();
            
            UpdateChunks(_texturesToUpdate);

            _texturesToUpdate.Clear();
            
            if (_realChunkPixels.IsCreated)
                _realChunkPixels.Dispose();
            if (_brushPixels.IsCreated)
                _brushPixels.Dispose();
            if (_tempChunkPixels.IsCreated)
                _tempChunkPixels.Dispose();
            if (_tempChunkMask.IsCreated)
                _tempChunkMask.Dispose();
        }
    }
    
    void UpdateChunks(List<Texture2D> updatableTextures)
    {
        for (int i = 0; i < updatableTextures.Count; i++)
        {
            updatableTextures[i].Apply(false);
        }
    }
   
   private Dictionary<Vector2Int, CanvasChunk> GetChunksDict()
   {
       if (!chunksDict.TryGetValue("layername", out var chunksdict))
       {
           chunksdict = new Dictionary<Vector2Int, CanvasChunk>();
           chunksDict.Add("layername", chunksdict);
       } 
       
       return chunksdict;
   }

   private CanvasChunk CreateChunkAt(int x, int y, Dictionary<Vector2Int, CanvasChunk> dict, float zPos = 0)
   {
      var unitSize = _chunkSize / (float) ArtHelper.HalfPixelsPerUnit;
      var parent = transform;
      var canvas = Instantiate(RawImage, parent);
      canvas.name = "Canvas " + x + " " + y;
      canvas.transform.localPosition = new Vector3(x * unitSize + unitSize / 2f, y * unitSize + unitSize / 2f, zPos);
      canvas.transform.localScale = new Vector3(unitSize, unitSize, 1);
      var drawer = canvas.GetComponent<CanvasChunk>();
      dict.Add(new Vector2Int(x, y), drawer);
      
      return drawer;
   }
   
    private void PaintArtChunk(int posX, int posY)
    {
        unsafe
        {
            _tempChunkMask =
                new UnsafeList<UnsafeList<UINTColor32>>(CurrentBrush.Data.Length / _chunkSize / _chunkSize,
                    Allocator.TempJob);
            _tempChunkPixels =
                new UnsafeList<UnsafeList<UINTColor32>>(CurrentBrush.Data.Length / _chunkSize / _chunkSize,
                    Allocator.TempJob);
            _brushPixels = new UnsafeList<UnsafeList<UINTColor32>>(CurrentBrush.Data.Length / _chunkSize / _chunkSize,
                Allocator.TempJob);
            
            _brushMask = new UnsafeList<UnsafeList<uint>>(CurrentBrush.Data.Length / _chunkSize / _chunkSize,
                Allocator.TempJob);
            
            var width = CurrentBrush.ActiveTexture.width - 1;
            var height = CurrentBrush.ActiveTexture.height - 1;

            var bottomBounds = new Vector2Int
            {
                x = posX,
                y = posY
            };

            var topBounds = new Vector2Int
            {
                x = posX + width,
                y = posY + height
            };

            var layerChunks = GetChunksDict();


            int log = (int)math.log2(_chunkSize);
            
            for (var x = bottomBounds.x >> log; x <= topBounds.x >> log; x++)
            for (var y = bottomBounds.y >> log; y <= topBounds.y >> log; y++)
            {
                if (!layerChunks.TryGetValue(new Vector2Int(x, y), out var realDrawer))
                {
                    realDrawer = CreateChunkAt(x, y, layerChunks);
                    realDrawer.Init(_chunkSize);
                }
                
                if (!_tempChunkDict.TryGetValue(new Vector2Int(x, y), out var drawer))
                {
                    drawer = CreateChunkAt(x, y, _tempChunkDict, -1);
                    drawer.Init(_chunkSize);
                }
                
                _texturesToUpdate.Add(drawer.texture);
                
                // Creates a NativeArray of RawTextureData Dispose of immediately after use
                var chunkData = drawer.texture.GetRawTextureData<UINTColor32>();
                // Access to a manually created maskData,dont dispose of this until canvas is destroyed
                var tempChunkMaskData = drawer.chunkInfo.maskData;

                var startX = bottomBounds.x - x * _chunkSize;
                startX &= ~startX >> 31;

                var endX = x * _chunkSize + _chunkSize - (topBounds.x + 1);
                endX &= ~endX >> 31;
                endX = _chunkSize - endX;

                var startY = (bottomBounds.y - y * _chunkSize) * _chunkSize;
                startY &= ~startY >> 31;

                var endY = (y * _chunkSize + _chunkSize - (topBounds.y + 1)) * _chunkSize;
                endY &= ~endY >> 31;
                endY = _chunkSize * _chunkSize - endY;

                var extraX = x * _chunkSize - bottomBounds.x;
                extraX &= ~extraX >> 31;
                var extraY = y * _chunkSize - bottomBounds.y;
                extraY &= ~extraY >> 31;

                var splitAmount = _chunkSize;
                var rowIndex = 0;

                for (var k = startY; k < endY; k += splitAmount)
                {
                    var pos = new Vector2Int(Mathf.CeilToInt(extraX),
                        Mathf.CeilToInt(rowIndex + extraY));

                    var brushYStart = pos.y * CurrentBrush.ActiveTexture.width + pos.x;

                    _tempChunkPixels.Add(new UnsafeList<UINTColor32>(
                        (UINTColor32*) chunkData.GetUnsafePtr() + (k + startX),
                        endX - startX));
                    _tempChunkMask.Add(new UnsafeList<UINTColor32>(
                        (UINTColor32*) tempChunkMaskData.GetUnsafePtr() + (k + startX),
                        endX - startX));
                    _brushPixels.Add(new UnsafeList<UINTColor32>((UINTColor32*) CurrentBrush.Data.GetUnsafePtr() + brushYStart,
                        endX - startX));
                    
                    _brushMask.Add(new UnsafeList<uint>((uint*) CurrentBrush.MaskData.GetUnsafePtr() + brushYStart,
                        endX - startX));
                    

                    rowIndex++;
                }

                chunkData.Dispose();
            }
        }

        paintModificationJob = new BoringJobFor
        {
            TempChunkPixels = _tempChunkPixels,
            TempChunkMask = _tempChunkMask,
            BrushPixels = _brushPixels,
            BrushMask = _brushMask,

            Color = CurrentBrush.Color
        };

        paintModificationHandle =
            paintModificationJob.Schedule(_brushPixels.Length, _brushPixels.Length / 8);
        
        paintModificationHandle.Complete();

        _tempChunkMask.Dispose();
        _tempChunkPixels.Dispose();
        _brushPixels.Dispose();
        _brushMask.Dispose();
    }
    
     void PaintTempToReal()
    {
        if (CurrentBrush.Color.a <= 0)
        {
            return;
        }
        
        unsafe
        { 
            _realChunkPixels = new UnsafeList<UnsafeList<UINTColor32>>(this.CurrentBrush.Data.Length / _chunkSize / _chunkSize, Allocator.TempJob);
            
            _tempChunkPixels = new UnsafeList<UnsafeList<UINTColor32>>(_tempChunkDict.Count * _chunkSize, Allocator.TempJob);
            
            _tempChunkMask = new UnsafeList<UnsafeList<UINTColor32>>(_tempChunkDict.Count * _chunkSize, Allocator.TempJob);

            
            _texturesToUpdate = new List<Texture2D>();

            foreach (Vector2Int pos in _tempChunkDict.Keys)
            {
                CanvasChunk tempDrawer = _tempChunkDict[pos];
                CanvasChunk realDrawer = GetChunksDict()[pos];

                _texturesToUpdate.Add(realDrawer.texture);

                NativeArray<UINTColor32> tempChunkData = tempDrawer.texture.GetRawTextureData<UINTColor32>();
                NativeArray<UINTColor32> tempChunkMaskData = tempDrawer.chunkInfo.maskData;
                
                NativeArray<UINTColor32> chunkData = realDrawer.texture.GetRawTextureData<UINTColor32>();

                int splitAmount = _chunkSize;

                int startY = 0;
                int endY = splitAmount * splitAmount;
                for (int k = startY; k < endY; k += splitAmount)
                {
                    _realChunkPixels.Add(new UnsafeList<UINTColor32>((UINTColor32*) chunkData.GetUnsafePtr() + k,
                        splitAmount));
                    _tempChunkPixels.Add(new UnsafeList<UINTColor32>((UINTColor32*) tempChunkData.GetUnsafePtr() + k,
                        splitAmount));
                    _tempChunkMask.Add(new UnsafeList<UINTColor32>((UINTColor32*) tempChunkMaskData.GetUnsafePtr() + k,
                        splitAmount));
                }
                /*tempChunkData.Dispose();
                tempChunkMaskData.Dispose();
                chunkData.Dispose();*/
                //chunkData.Dispose();
            }

            paintToRealJob = new PaintToRealJob()
            {
                RealChunkPixels = _realChunkPixels,
                TempChunkPixels = _tempChunkPixels,
                TempChunkMask = _tempChunkMask,

                Color = CurrentBrush.Color

            };

            paintToRealModificationHandle =
                paintToRealJob.Schedule(_realChunkPixels.Length, _realChunkPixels.Length / 8);
            
            paintToRealModificationHandle.Complete();

            _tempChunkMask.Dispose();
            _tempChunkPixels.Dispose();
            _realChunkPixels.Dispose();
        }
    }
     
     void ResetTemp()
     {
         foreach (var drawer in (_tempChunkDict.Values))
         {
             Destroy(drawer.gameObject);
         }
        
         _tempChunkDict.Clear();
     }
    
     [BurstCompile]
     private struct BoringJobFor : IJobParallelFor
    {
        [ReadOnly] public UnsafeList<UnsafeList<UINTColor32>> TempChunkPixels;
        [ReadOnly] public UnsafeList<UnsafeList<UINTColor32>> TempChunkMask;
        [ReadOnly] public UnsafeList<UnsafeList<UINTColor32>> BrushPixels;
        [ReadOnly] public UnsafeList<UnsafeList<uint>> BrushMask;

        [ReadOnly] public UINTColor32 Color;

        public void Execute(int i)
        {
            var paintColor = Color;
            
            var tempChunkPixels = TempChunkPixels[i];
            var tempChunkMaskData = TempChunkMask[i];
            var brushPixel = BrushPixels[i];
            var brushMask = BrushMask[i];

            var maxColor = new UINTColor32(255, 255, 255, 255);

            for (var j = 0; j < tempChunkPixels.Length; j++)
            {
                UINTColor32 brushMaskColor = brushPixel[j];
                uint brushMaskMask = brushMask[j];
                
                paintColor.SetRGBA(tempChunkPixels[j].Rgba & brushMaskMask);
                
                paintColor.SetRGBA(paintColor.Rgba | brushMaskColor.Rgba);
                
                tempChunkPixels[j] = paintColor;
                tempChunkMaskData[j] = maxColor;
            }

            brushPixel.Dispose();
            brushMask.Dispose();
            tempChunkMaskData.Dispose();
            tempChunkPixels.Dispose();
        }
    }
     
      [BurstCompile]
    public struct PaintToRealJob : IJobParallelFor
    {
        [ReadOnly]
        public UnsafeList<UnsafeList<UINTColor32>> RealChunkPixels;
        [ReadOnly]
        public UnsafeList<UnsafeList<UINTColor32>> TempChunkPixels;
        [ReadOnly]
        public UnsafeList<UnsafeList<UINTColor32>> TempChunkMask;

        [ReadOnly]
        public UINTColor32 Color;

        public void Execute(int i)
        {
            UINTColor32 paintColor = Color;
            
            UnsafeList<UINTColor32> realChunkPixel = this.RealChunkPixels[i];
            UnsafeList<UINTColor32> tempChunkPixel = this.TempChunkPixels[i];
            UnsafeList<UINTColor32> tempChunkMask = this.TempChunkMask[i];

            for (int j = 0; j < tempChunkPixel.Length; j++)
            {
                UINTColor32 newColor = Color;
                UINTColor32 tempColor = tempChunkPixel[j];

                newColor.SetRGBA(PR3AlphaCombine(tempColor.Rgba, realChunkPixel[j].Rgba));

                UINTColor32 prev = new UINTColor32();
                prev.SetRGBA((realChunkPixel[j].Rgba & ~tempChunkMask[j].Rgba));
                
                newColor.SetRGBA(newColor.Rgba & tempChunkMask[j].Rgba);
                paintColor.SetRGBA(prev.Rgba | newColor.Rgba);

                realChunkPixel[j] = paintColor;
            }

            tempChunkPixel.Dispose();
            tempChunkMask.Dispose();
                
            realChunkPixel.Dispose();
        }

        uint PR3AlphaCombine(uint color, uint oldColor)
        {
            float oldAlphaAmount = (oldColor >> 24 & 255) / 255f;
            
            float alpha = (byte)(color >> 24 & 255) / 255f;
            float amount = 1.0f - alpha;
            float alphaCombined = oldAlphaAmount * amount;
            float newAlpha = oldAlphaAmount * amount + alpha;

            uint newColor = (uint)Math.Round(newAlpha * 255) << 24
                            | (uint)Math.Round(((oldColor >> 16 & 255) * alphaCombined + (color >> 16 & 255) * alpha) / newAlpha) << 16
                            | (uint)Math.Round(((oldColor >> 8 & 255) * alphaCombined + (color >> 8 & 255) * alpha) / newAlpha) << 8
                            | (uint)Math.Round(((oldColor & 255) * alphaCombined + (color & 255) * alpha) / newAlpha);

            return newColor;
        }
    }
}
