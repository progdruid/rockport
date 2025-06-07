using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Menu
{

public class MenuBehaviour : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private SequentialSoundPlayer soundPlayer;
    [SerializeField] private TransitionVeil transitionVeil;
    [SerializeField] private ParallaxBackground[] parallaxLayers;
    [SerializeField] private Camera menuCamera;
    [SerializeField] private float cameraGlideSpeed = 1f;
    [SerializeField] private float groundHeight = -5f;
    [SerializeField] private int maxLevel = 6;
    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private string upcomingLevelPhrase = "soon";
    
    private int _currentLevel = 1;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(transitionVeil);
        foreach (var layer in parallaxLayers) 
            Assert.IsNotNull(layer);
        Assert.IsNotNull(menuCamera);
        Assert.IsNotNull(currentLevelText);
    }
    
    private void Start()
    {
        soundPlayer?.StartPlaying();
        transitionVeil.TransiteOut().Start(this);
        foreach (var layer in parallaxLayers)
        {
            layer.SetTarget(menuCamera.transform);
            layer.SetGroundLevel(groundHeight);
        }
        
        if (PlayerPrefs.HasKey("LoadedMap"))
        {
            var numberSubstring = PlayerPrefs.GetString("LoadedMap").Substring(5);
            if (int.TryParse(numberSubstring, out var level) && level > 0 && level <= maxLevel)
            {
                _currentLevel = level;
                currentLevelText.text = "lvl " + _currentLevel;
            }
        }
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void GoToGameplay()
    {
        if (_currentLevel == maxLevel + 1) return;
        
        routine().Start(this);
        return;
        IEnumerator routine()
        {
            transitionVeil.TransiteIn().Start(this);
            if (soundPlayer)
                yield return soundPlayer.StopPlaying();
            yield return new WaitWhile(() => transitionVeil.inTransition);
        
            PlayerPrefs.SetString("LoadedMap", "Level" + _currentLevel);
            SceneManager.LoadScene("MapGameplay");
        }
    }

    public void GoToEditor()
    {
        routine().Start(this);
        return;
        IEnumerator routine()
        {
            transitionVeil.TransiteIn().Start(this);
            if (soundPlayer)
                yield return soundPlayer.StopPlaying();
            yield return new WaitWhile(() => transitionVeil.inTransition);
        
            PlayerPrefs.SetString("LoadedMap", "Level" + _currentLevel);
            SceneManager.LoadScene("MapEditor");
        }
    }

    public void ChangeSelectedMap(int direction)
    {
        var newLevel = _currentLevel + direction;
        if (newLevel < 1 || newLevel > maxLevel + 1) return;
        _currentLevel = newLevel;
        currentLevelText.text = 
            _currentLevel == maxLevel + 1 
                ? upcomingLevelPhrase 
                : "lvl " + _currentLevel;
    }
    
    
    
    //game loop/////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        menuCamera.transform.position += Time.deltaTime * cameraGlideSpeed * Vector3.right;
        
        if (Input.GetKeyDown(KeyCode.E))
            GoToEditor();
    }
}

}