using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum BlockType
{
    None,
    Dirt,
    Wood,
    Planks
}

[System.Serializable]
public struct BlockData
{
    public Vector2Int pos;
    public BlockType type;
    public byte collisionAmbiguityValue;
    public byte fillingAmbiguityValue;
}
