using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArtHelper
{
   public static int HalfPixelsPerUnit = 64;
   
   public static Vector2Int WorldSpaceTest(Vector2 worldPos, Texture2D brushTexture, int chunkSize)
   {
      Vector2 middleOfTexture = new Vector2(1, 0);
      Vector2 pixelsPos = WorldSpaceToPixelsNoRounding(worldPos, brushTexture, chunkSize);

      
      return Vector2Int.RoundToInt(pixelsPos - middleOfTexture);
   }
   
   public static Vector2Int WorldSpaceCenter(Vector2 worldPos, Texture2D brushTexture, int chunkSize)
   {
      Vector2 middleOfTexture = new Vector2((brushTexture.width / 2f) - .5f, (brushTexture.height / 2f) - .5f);
      Vector2 pixelsPos = WorldSpaceToPixelsNoRounding(worldPos, brushTexture, chunkSize) - middleOfTexture;
      
      return Vector2Int.RoundToInt(pixelsPos);
      //return Vector2Int.RoundToInt(pixelsPos - middleOfTexture);
   }

   public static Vector2 PixelsToWorld(Vector2Int pixelsPos)
   {
      float pixelsUnitSize = (float)HalfPixelsPerUnit;
      
      Vector2 convertedBack = new Vector2(pixelsPos.x / pixelsUnitSize, pixelsPos.y / pixelsUnitSize);

      return convertedBack;
   }

   public static Vector2Int WorldSpaceToPixelsWithRounding(Vector2 worldPos, Texture2D brushTexture, int chunkSize)
   {
      Vector2Int texSize = new Vector2Int(brushTexture.width, brushTexture.height);
      float pixelsUnitSize = (float)HalfPixelsPerUnit;
      float pixelsInWorldSpace = (1f / chunkSize);
      Vector2Int pos = new Vector2Int(Mathf.RoundToInt((worldPos.x - pixelsInWorldSpace) * pixelsUnitSize),
         Mathf.RoundToInt((worldPos.y  - pixelsInWorldSpace) * pixelsUnitSize));

      return pos;
   }
   
   public static Vector2 WorldSpaceToPixelsNoRounding(Vector2 worldPos, Texture2D brushTexture, int chunkSize)
   {
      float pixelsUnitSize = (float)HalfPixelsPerUnit;
      float pixelsInWorldSpace = (1f / chunkSize);
      Vector2 pos = new Vector2((worldPos.x - pixelsInWorldSpace) * pixelsUnitSize,
         (worldPos.y  - pixelsInWorldSpace) * pixelsUnitSize);

      return pos;
   }
}
