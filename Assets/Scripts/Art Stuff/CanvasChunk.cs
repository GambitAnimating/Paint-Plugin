using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;

public class CanvasChunk : MonoBehaviour
{
    public struct ChunkInfo
    {
        public NativeArray<UINTColor32> data;
        public NativeArray<UINTColor32> maskData;
        public float2 _bottomLeft;
        public UnsafeList<UINTColor32> unsafeData;
    }
    public Texture2D texture;
    private int textureWidth;
    private int textureHeight;
    private RawImage rawImage;
    private SpriteRenderer renderer;

    public ChunkInfo chunkInfo;

    private bool initiated = false;

    public GameObject myGameObject;

    public void Init(int size)
    {
        
            if (initiated)
                return;
        
            texture = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            myGameObject = gameObject;



            chunkInfo.data = texture.GetRawTextureData<UINTColor32>();
            chunkInfo.maskData = new NativeArray<UINTColor32>(chunkInfo.data.Length, Allocator.Persistent);

            unsafe
            {
                chunkInfo.unsafeData =
                    new UnsafeList<UINTColor32>((UINTColor32*) chunkInfo.data.GetUnsafePtr(), chunkInfo.data.Length);
            }
            
            MakeTextureBlank();

            GetComponent<Renderer>().material.SetTexture("_MainTex", texture);

            this.textureWidth = texture.width;
            this.textureHeight = texture.height;

            chunkInfo._bottomLeft = transform.localScale.x / 2;

            Vector2 pos = this.transform.position;
            chunkInfo._bottomLeft = (float2)pos - chunkInfo._bottomLeft;

            initiated = true;
    }

    private void OnDestroy()
    {
        if (chunkInfo.maskData.IsCreated)
            chunkInfo.maskData.Dispose();
        if (chunkInfo.unsafeData.IsCreated)
            chunkInfo.unsafeData.Dispose();
    }

    void MakeTextureBlank()
    {
        UINTColor32 clear = new UINTColor32(0, 0, 0, 0);
        for (int i = 0; i < chunkInfo.data.Length; i++)
        {
            chunkInfo.data[i] = clear;
        }
        texture.Apply(false);
    }
}
