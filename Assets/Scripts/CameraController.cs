using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
   
   [SerializeField] private float speed = 5;
   [SerializeField] private float zoomIncreaseAmount = 5;
   private float _zPos = 0;

   private Camera _cam;

   private void Awake()
   {
      _cam = GetComponent<Camera>();
      _zPos = transform.position.z;
   }

   private void Update()
   {
      float tempSpeed = this.speed;
      float zoomIncrease = zoomIncreaseAmount;
      if (Input.GetKey(KeyCode.LeftShift))
      {
         tempSpeed *= 4;
         zoomIncrease *= 4;
      }

      tempSpeed *= Time.deltaTime;
      zoomIncrease *= Time.deltaTime;
      if (Input.GetKey(KeyCode.A))
      {
         this.transform.position = new Vector3(this.transform.position.x - tempSpeed, this.transform.position.y, _zPos);
      }

      if (Input.GetKey(KeyCode.D))
      {
         this.transform.position = new Vector3(this.transform.position.x + tempSpeed, this.transform.position.y, _zPos);
      }

      if (Input.GetKey(KeyCode.W))
      {
         this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + tempSpeed, _zPos);
      }

      if (Input.GetKey(KeyCode.S))
      {
         this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - tempSpeed, _zPos);
      }

      if (Input.GetKey(KeyCode.Q))
      {
         _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize + zoomIncrease, .01f, Mathf.Infinity);
      }

      if (Input.GetKey(KeyCode.E))
      {
         _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize - zoomIncrease, .01f, Mathf.Infinity);
      }
   }
}
