using System.Collections;
using System.Collections.Generic;

public class GameSystems 
{
    public static GameSystems Ins { get; private set; } = new GameSystems();

    public MapLoader Loader;
    public GameplayCamera GameplayCamera;
    public InputSet InputSet;
    public CorpseManager CorpseManager;
    public FruitManager FruitManager;
    public PlayerManager PlayerManager;

    public DeathsBarManager DeathsBar;
    public TransitionVeil TransitionVeil;
}
