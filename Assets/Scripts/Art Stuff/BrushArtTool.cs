using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BrushArtTool : ArtTool
{
    public BrushArtTool(ArtManager artManager) : base(artManager) { }

    private Vector2Int _lastDrawPixelPos;
    private Vector2 _lastPixelPosRot;
    
    private float _pixelDistance = 3;

    private float rot = 0;

    public override void OnMouseHeld(Vector3 mousePos)
    {
        Vector2Int pixelPos = ArtHelper.WorldSpaceCenter(mousePos, _artManager._brushTexture, 128);
        
        Vector2 dir = (pixelPos - _lastDrawPixelPos);
        dir.Normalize();
        
        _artManager.GetCurrentCanvas().SetupBrushData(_artManager._brushTexture);
        _artManager.GetCurrentCanvas().DrawToCanvas(pixelPos);
        
        // TODO: Create Line of texture from start and ending mouse position to make it continuous
    }

    public override void OnMouseDown(Vector3 mousePos)
    {
        
        // Texture2D brush = TransformationsHelper.NewNewRotate(_artManager._brushTexture, 0, false);
        // _artManager.GetCurrentCanvas().SetupBrushData(brush);
        // Vector2Int pixelPos = ArtHelper.WorldSpaceCenter(Vector2.one, brush, 128);
        // Debug.Log("PixelPos: " + pixelPos);
        // _artManager.GetCurrentCanvas().DrawToCanvas(pixelPos);
    }

    public override void OnMouseUp(Vector3 mousePos)
    {
        _artManager.GetCurrentCanvas().FinishDraw();
    }
}
