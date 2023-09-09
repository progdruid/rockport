using System.Collections;
using System.Collections.Generic;

public class Registry 
{
    public static Registry ins { get; private set; } = new Registry();

    public LevelLoader lm;
    public CameraManager cameraManager;
    public InputSet inputSet;
    public CorpseManager corpseManager;
    public FruitManager fruitManager;
    public PlayerManager playerManager;

    public DeathsBarManager deathsBar;
    public TransitionVeil transitionVeil;
}
