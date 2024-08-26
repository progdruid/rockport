using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct PlacingDecision
{
    public TileBase BG;
    public TileBase Matching;
    public TileBase FillingDeep;
    public TileBase FillingShallow;
}


public abstract class BlockPlacingScript : ScriptableObject
{
    public abstract void RepaintSingle(LevelHolder holder, Vector2Int pos);
    public abstract void RepaintEnvironment(LevelHolder holder, Vector2Int pos);
}
