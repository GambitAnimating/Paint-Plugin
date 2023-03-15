using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtManager : MonoBehaviour
{
   [field: SerializeField] public Texture2D _brushTexture { get; private set; }
   
   private ArtTool _artTool;
   [SerializeField] private Canvas _currentCanvas;

   private void Awake()
   {
      _artTool = new BrushArtTool(this);
   }

   private void Start()
   {
      MouseInfo.InitMouse();
   }

   void Update()
   {
      HandleMouse();
   }

   void HandleMouse()
   {
      Vector3 worldMousePos = MouseInfo.GetWorldMousePos();
      
      MouseInfo.UpdateAngle();
      
      if (Input.GetMouseButton(0))
      {
         _artTool.OnMouseHeld(worldMousePos);
      }
      if (Input.GetMouseButtonDown(0))
      {
         _artTool.OnMouseDown(worldMousePos);
      }
      else if (Input.GetMouseButtonUp(0))
      {
         _artTool.OnMouseUp(worldMousePos);
      }

      MouseInfo.SetLastWorldPos(worldMousePos);
   }
   
   public Canvas GetCurrentCanvas()
   {
      return _currentCanvas;
   }
}
