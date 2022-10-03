using System.Collections;
using System.Collections.Generic;

public class Registry 
{
    #region static
    public static Registry ins { get; private set; }

    public static void Init ()
    {
        ins = new Registry ();
    }
    #endregion

    public LevelManager lm;
    public TransitionController tc;
    public InputSystem inputSystem;
    public CorpseManager corpseManager;
    public SkullManager skullManager;

    public DeathsBarManager deathsBar;

    public Player player;
}
