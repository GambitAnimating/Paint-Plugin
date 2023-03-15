using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MouseInfo
{
    public static Vector2 _lastWorldPos { get; private set; }
    
    public static Vector2 _lastAngleMousePos { get; private set; }
    
    public static int _mouseAngle { get; private set; }

    public static void InitMouse()
    {
        _lastWorldPos = GetWorldMousePos();
    }

    public static Vector3 GetWorldMousePos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 2;

        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        return worldMousePos;
    }

    public static void SetLastWorldPos(Vector2 lastWorldPos) => _lastWorldPos = lastWorldPos;

    public static void UpdateAngle()
    {
        Vector2 dir = (Vector2)GetWorldMousePos() - _lastAngleMousePos;
        if (Vector2.Distance((Vector2)GetWorldMousePos(), _lastAngleMousePos) >= .2f)
        {
            dir.Normalize();
            
            Debug.Log("Set new dir " + dir);
           
        
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            
            _mouseAngle = (int)angle;
            
            Debug.Log("Angle " + _mouseAngle);

            _lastAngleMousePos = (Vector2) GetWorldMousePos();
        }
    }
}
