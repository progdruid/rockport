using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystems 
{
    public static GameSystems Ins { get; private set; } = new GameSystems();

    public MapManager MapManager;
    public GameplayCamera GameplayCamera;
    public CorpseManager CorpseManager;
    public FruitManager FruitManager;
    public PlayerManager PlayerManager;

    public TransitionVeil TransitionVeil;
    public PauseUIManager PauseUIManager;  
}
