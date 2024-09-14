using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelPass : ScriptableObject
{
    protected LevelHolder _levelHolder;
    
    public virtual void InjectLevelHolder(LevelHolder holder) => _levelHolder = holder;
}
