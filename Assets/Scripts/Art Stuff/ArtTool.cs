using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ArtTool
{
    protected readonly ArtManager _artManager;

    protected ArtTool(ArtManager artManager)
    {
        _artManager = artManager;
    }
    
    public abstract void OnMouseHeld(Vector3 mousePos);
    public abstract void OnMouseDown(Vector3 mousePos);
    public abstract void OnMouseUp(Vector3 mousePos);
}
