using System;
using BrutalUI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace MapGameplay
{

public class HUDSystem : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private Image[] controlImages;
    [SerializeField] private BrutalToggle controlToggle;
    
    private bool _isShowing;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        foreach (var controlImage in controlImages)
            Assert.IsNotNull(controlImage);
        Assert.IsNotNull(controlToggle);
        
        if (PlayerPrefs.HasKey("ShowControls")) 
            UpdateShow(PlayerPrefs.GetInt("ShowControls") == 1);
        else
            UpdateShow(true);
    }

    private void Start()
    {
        controlToggle.SetValue(_isShowing);
        controlToggle.Subscribe(UpdateShow);
    }

    private void OnDestroy() => controlToggle.Unsubscribe(UpdateShow);

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateShow(bool value)
    {
        _isShowing = value;
        PlayerPrefs.SetInt("ShowControls", _isShowing ? 1 : 0);
        PlayerPrefs.Save();
        
        foreach (var controlImage in controlImages)
            controlImage.gameObject.SetActive(_isShowing);
    }
}

}