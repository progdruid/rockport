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
    public TransitionController transitionController;
    public InputSystem inputSystem;
    public CorpseManager corpseManager;
    public SkullManager skullManager;
    
}
