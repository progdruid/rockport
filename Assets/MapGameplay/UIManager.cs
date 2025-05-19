using System;
using BrutalUI;
using UnityEngine;
using UnityEngine.UI;

namespace MapGameplay
{

public class UIManager : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private BrutalImageColorTweener dimImageTweener;
    [SerializeField] private BrutalABTweener[] tweeners;

    
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            foreach (var tweener in tweeners)
                tweener.TweenToStart();
            dimImageTweener.TweenToStart();
        }
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            foreach (var tweener in tweeners)
                tweener.TweenToTarget();
            dimImageTweener.TweenToTarget();
        }

    }

}

}