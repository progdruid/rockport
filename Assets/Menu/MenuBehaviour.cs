using System;
using System.Collections;
using System.Collections.Generic;
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
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(soundPlayer);
        Assert.IsNotNull(transitionVeil);
        foreach (var layer in parallaxLayers) 
            Assert.IsNotNull(layer);
        Assert.IsNotNull(menuCamera);
    }
    
    private void Start()
    {
        soundPlayer.StartPlaying();
        transitionVeil.TransiteOut().Start(this);
        foreach (var layer in parallaxLayers)
        {
            layer.SetTarget(menuCamera.transform);
            layer.SetGroundLevel(groundHeight);
        }
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void HandlePlayButton()
    {
        HandlePlayButtonRoutine().Start(this);
    }

    private IEnumerator HandlePlayButtonRoutine()
    {
        transitionVeil.TransiteIn().Start(this);
        yield return soundPlayer.StopPlaying();
        yield return new WaitWhile(() => transitionVeil.inTransition);

        //PlayerPrefs.SetInt("Level_ID_Selected_in_Menu", levelListMover.GetSelectedLevel());
        SceneManager.LoadScene("MapGameplay");
    }

    public void HandleQuitButton()
    {
        Application.Quit();
    }
    
    
    //game loop/////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        menuCamera.transform.position += Time.deltaTime * cameraGlideSpeed * Vector3.right;
    }
}

}