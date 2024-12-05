using System.Collections;
using System.Collections.Generic;

public class GameSystems 
{
    public static GameSystems ins { get; private set; } = new GameSystems();

    public ChapterLoader lm;
    public CameraManager cameraManager;
    public InputSet inputSet;
    public CorpseManager corpseManager;
    public FruitManager fruitManager;
    public PlayerManager playerManager;

    public DeathsBarManager deathsBar;
    public TransitionVeil transitionVeil;
}
