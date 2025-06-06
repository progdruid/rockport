using System;
using System.Collections;
using BrutalUI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class GameplayUISystem : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Tweeners")]
    [SerializeField] private BrutalImageColorTweener dimImageTweener;
    [SerializeField] private BrutalABTweener[] tweeners;
    [Header("HUD Control")]
    [SerializeField] private BrutalToggle helpToggle;
    [SerializeField] private Image[] controlImages;
    [Header("Audio Control")]
    [SerializeField] private BrutalSlider musicSlider;
    [SerializeField] private BrutalSlider sfxSlider;
    [Header("HUD Bar")] 
    [SerializeField] private GameObject fruitBar;
    [SerializeField] private GameObject pausePanel;
    
    private bool _isShowing;
    private bool _hudBarVisible = true;
    
    private float _musicVolume;
    private float _sfxVolume;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(dimImageTweener);
        foreach (var tweener in tweeners)
            Assert.IsNotNull(tweener);
        
        Assert.IsNotNull(helpToggle);
        foreach (var controlImage in controlImages)
            Assert.IsNotNull(controlImage);
        
        Assert.IsNotNull(musicSlider);
        Assert.IsNotNull(sfxSlider);
        
        Assert.IsNotNull(fruitBar);
        Assert.IsNotNull(pausePanel);
        
        //controls
        UpdateHUD(!PlayerPrefs.HasKey("ShowControls") 
                  || PlayerPrefs.GetInt("ShowControls") == 1);
        
        //audio
        GameSystems.Ins.GameplayUISystem = this;
    }

    private void Start()
    {
        dimImageTweener.SnapToTarget();
        dimImageTweener.gameObject.SetActive(false);
        foreach (var tweener in tweeners)
            tweener.SnapToTarget();
        
        helpToggle.SetValue(_isShowing);
        helpToggle.Subscribe(UpdateHUD);
        
        UpdateMusic(PlayerPrefs.HasKey("MusicVolume") 
            ? PlayerPrefs.GetFloat("MusicVolume") 
            : 1f);

        UpdateSFX(PlayerPrefs.HasKey("SFXVolume") 
            ? PlayerPrefs.GetFloat("SFXVolume") 
            : 1f);
        
        
        musicSlider.SetValue(_musicVolume);
        musicSlider.Subscribe(UpdateMusic);
        
        sfxSlider.SetValue(_sfxVolume);
        sfxSlider.Subscribe(UpdateSFX);

    }
    private void OnDestroy() => helpToggle.Unsubscribe(UpdateHUD);
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void HandleOpenButton()
    {
        GameSystems.Ins.Controller.SetAllowMove(false);
        Show();
    }
    
    public void HandleQuitButton()
    {
        routine().Start(this);
        return;

        IEnumerator routine()
        {
            GameSystems.Ins.GameplayUISystem.Hide();
            yield return WaitUntilHidden();
            GameSystems.Ins.MapManager.QuitToScene("MainMenu");
        }
    }
    
    public void HandleRestartButton()
    {
        routine().Start(this);
        return;

        IEnumerator routine()
        {
            GameSystems.Ins.GameplayUISystem.Hide();
            yield return WaitUntilHidden();
            GameSystems.Ins.MapManager.ReloadMap();
        }
    }
    
    public void HandleContinueButton()
    {
        routine().Start(this);
        return;
        
        IEnumerator routine() {
            Hide();
            yield return WaitUntilHidden();
            GameSystems.Ins.Controller.SetAllowMove(true);
        }
    }

    public void ToggleHUDBar()
    {
        _hudBarVisible = !_hudBarVisible;
        fruitBar.SetActive(_hudBarVisible);
        pausePanel.SetActive(_hudBarVisible);
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
    
    private void UpdateHUD(bool value)
    {
        _isShowing = value;
        PlayerPrefs.SetInt("ShowControls", _isShowing ? 1 : 0);
        PlayerPrefs.Save();
        
        foreach (var controlImage in controlImages)
            controlImage.gameObject.SetActive(_isShowing);
    }

    private void UpdateMusic(float value)
    {
        _musicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
        
        SequentialSoundPlayer.UpdateGlobalVolume();
    }
    
    private void UpdateSFX(float value)
    {
        _sfxVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
        
        CustomSoundEmitter.UpdateGlobalVolume();
        PermutableSoundPlayer.UpdateGlobalVolume();
    }
}