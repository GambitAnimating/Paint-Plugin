using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
 
public class ShowFPS : MonoBehaviour {
    public Text fpsText;
    public Text pixelText;
    public Text blockCountText;
    public Text resolution;
    public float deltaTime;
    void Update () {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = Mathf.Ceil (fps).ToString ();
        /*resolution.text = "Total Pixel width In Brush Size: " + PainterNew.Instance.GetBrushWidth();
        pixelText.text = "Total Pixels In Brush Size: " + PainterNew.Instance.GetBrushPixelsRendererd();
        blockCountText.text = "Total Blockcount: " + PainterNew.Instance.GetBlockCount();*/
    }
}