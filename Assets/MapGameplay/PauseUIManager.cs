using System;
using System.Collections;
using BrutalUI;
using UnityEngine;
using UnityEngine.Assertions;

public class PauseUIManager : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private BrutalImageColorTweener dimImageTweener;
    [SerializeField] private BrutalABTweener[] tweeners;

    [SerializeField] private GameplayController controller;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(dimImageTweener);
        foreach (var tweener in tweeners)
            Assert.IsNotNull(tweener);
        
        GameSystems.Ins.PauseUIManager = this;
    }

    private void Start()
    {
        dimImageTweener.SnapToTarget();
        dimImageTweener.gameObject.SetActive(false);
        foreach (var tweener in tweeners)
            tweener.SnapToTarget();
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void HandleOpenButton()
    {
        controller.AllowMove = false;
        Show();
    }
    
    public void HandleQuitButton()
    {
        routine().Start(this);
        return;

        IEnumerator routine()
        {
            GameSystems.Ins.PauseUIManager.Hide();
            yield return WaitUntilHidden();
            GameSystems.Ins.MapManager.QuitToMenu();
        }
    }
    
    public void HandleRestartButton()
    {
        routine().Start(this);
        return;

        IEnumerator routine()
        {
            GameSystems.Ins.PauseUIManager.Hide();
            yield return WaitUntilHidden();
            GameSystems.Ins.MapManager.ReloadLevel();
        }
    }
    
    public void HandleContinueButton()
    {
        routine().Start(this);
        return;
        
        IEnumerator routine() {
            Hide();
            yield return WaitUntilHidden();
            controller.AllowMove = true;
        }
    }
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Show()
    {
        dimImageTweener.gameObject.SetActive(true);
        dimImageTweener.TweenToStart();
        foreach (var tweener in tweeners)
            tweener.TweenToStart();
    }

    private IEnumerator WaitUntilShown()
    {
        yield return new WaitUntil(dimImageTweener.IsAtStart);
        foreach (var tweener in tweeners)
            yield return new WaitUntil(tweener.IsAtStart);
    }

    private void Hide()
    {
        routine().Start(this);
        return;
        
        IEnumerator routine()
        {
            foreach (var tweener in tweeners)
                tweener.TweenToTarget();
            dimImageTweener.TweenToTarget();
            yield return WaitUntilHidden();
            dimImageTweener.gameObject.SetActive(false);
        }
    }

    private IEnumerator WaitUntilHidden()
    {
        yield return new WaitUntil(dimImageTweener.IsAtTarget);
        foreach (var tweener in tweeners)
            yield return new WaitUntil(tweener.IsAtTarget);
    }
}