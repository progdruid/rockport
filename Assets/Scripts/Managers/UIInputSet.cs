using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInputSet : InputSet
{
    [SerializeField] RectTransform leftMoveRect;
    [SerializeField] RectTransform rightMoveRect;
    [SerializeField] RectTransform jumpRect;

    private bool jumpedBefore = false;

    private void Update()
    {
        int hor = 0;
        bool leftActivated = false;
        bool rightActivated = false;
        bool jumpActivated = false;

        for (int i = 0; i < Input.touches.Length; i++)
        {
            //use Camera.main instead of null if Canvas is in Camera mode
            //null for Overlay
            leftActivated = leftActivated || RectTransformUtility.RectangleContainsScreenPoint(leftMoveRect, Input.touches[i].position, Camera.main);
            rightActivated = rightActivated || RectTransformUtility.RectangleContainsScreenPoint(rightMoveRect, Input.touches[i].position, Camera.main);
            jumpActivated = jumpActivated || RectTransformUtility.RectangleContainsScreenPoint(jumpRect, Input.touches[i].position, Camera.main);
        }

        if (leftActivated)
            hor -= 1;
        else if (rightActivated)
            hor += 1;

        HorizontalValue = hor;

        if (jumpActivated && !jumpedBefore)
        {
            jumpedBefore = true;
            InvokeJumpKeyPressEvent();
        }
        else if (!jumpActivated && jumpedBefore)
        {
            jumpedBefore = false;
            InvokeJumpKeyReleaseEvent();
        }
    }
}
