using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PixelPerfectCameraScaler : MonoBehaviour
{
    private enum AdjustAxis
    { Horizontal, Vertical, }

    [SerializeField] AdjustAxis adjustAxis;

    void Start()
    {
        var pixelPerfectCam = GetComponent<PixelPerfectCamera>();
        if (adjustAxis == AdjustAxis.Horizontal)
        {
            float screenRatioHor = (float)(Screen.width) / (float)(Screen.height);
            int resX = Mathf.RoundToInt(pixelPerfectCam.refResolutionY * screenRatioHor);
            pixelPerfectCam.refResolutionX = resX;
        }
        else if (adjustAxis == AdjustAxis.Vertical)
        {
            float screenRatioVert = (float)(Screen.height) / (float)(Screen.width);
            int resY = Mathf.RoundToInt(pixelPerfectCam.refResolutionX * screenRatioVert);
            pixelPerfectCam.refResolutionY = resY;
        }
    }
}
