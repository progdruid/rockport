using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Registry : MonoBehaviour
{
    #region static
    public static Registry ins { get; private set; }

    private void Awake()
    {
        ins = this;
    }
    #endregion

    public LevelManager lm;
    public TransitionController transitionController;
    public InputSystem inputSystem;
    //public CorpseManager corpseManager;

    
}
