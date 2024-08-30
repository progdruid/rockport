using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;


[CreateAssetMenu(fileName = "", menuName = "Scriptables/DirtPlacingScript", order = 1)]
public class DirtPlacingScript : BlockPlacingScript
{
    [SerializeField] public SerializableMap<GameObject, GameObject> MyDict;
    
    [SerializeField] private TileBase BG_Outer;
    [SerializeField] private byte BG_MidStart;
    [SerializeField] private TileBase BG_Mid;
    [SerializeField] private byte BG_InnerStart;
    [SerializeField] private TileBase BG_Inner;
    
    [Space]
    [SerializeField] private TileBase[] Edges_Up;
    [SerializeField] private TileBase[] Edges_Down;
    [SerializeField] private TileBase[] Edges_Right;
    [SerializeField] private TileBase[] Edges_Left;
    
    [Space]
    [SerializeField] private TileBase Corner_UR;
    [SerializeField] private TileBase Corner_UL;
    [SerializeField] private TileBase Corner_DR;
    [SerializeField] private TileBase Corner_DL;

    [Space]
    [SerializeField] private TileBase[] Fillings_Deep_Outer;
    [SerializeField] private TileBase[] Fillings_Deep_Mid;
    [SerializeField] private TileBase[] Fillings_Deep_Inner;
    
    [Space]
    [SerializeField] private TileBase[] Fillings_Shallow_Outer;
    [SerializeField] private TileBase[] Fillings_Shallow_Mid;
    [SerializeField] private TileBase[] Fillings_Shallow_Inner;
    
    private Func<TileBase>[] MatchingGetters;
    
    private byte ambiguityValue = 0;

    private void OnValidate()
    {   
        MatchingGetters = new Func<TileBase>[]
        {
            () => null,
            () => Edges_Right[ambiguityValue % Edges_Right.Length],
            () => Edges_Left[ambiguityValue % Edges_Right.Length],
            () => null,
            () => Edges_Up[ambiguityValue % Edges_Right.Length],
            () => Corner_UR,
            () => Corner_UL,
            () => Edges_Up[ambiguityValue % Edges_Right.Length],
            () => Edges_Down[ambiguityValue % Edges_Right.Length],
            () => Corner_DR,
            () => Corner_DL,
            () => null,
            () => Edges_Up[ambiguityValue % Edges_Right.Length],
            () => Corner_UR,
            () => Corner_UL,
            () => Edges_Up[ambiguityValue % Edges_Right.Length]
        };
    }


    public override void RepaintSingle(LevelHolder holder, Vector2Int pos)
    {
        var depth = GetDepth(holder, pos);
        RepaintWithDepth(holder, pos, depth);
    }

    public override void RepaintEnvironment(LevelHolder holder, Vector2Int pos)
    {
        var mapSize = holder.GetMapSize();
        var radius= BG_InnerStart;
        var minX = Math.Max(pos.x - radius, 0);
        var maxX = Math.Min(pos.x + radius, mapSize.x - 1);
        var minY = Math.Max(pos.y - radius, 0);
        var maxY = Math.Min(pos.y + radius, mapSize.y - 1);
        
        for (var x = minX; x <= maxX; x++)
        for (var y = minY; y <= maxY; y++)
            if (holder.GetBlockTypeAt(x, y) == BlockType.Dirt) 
                RepaintSingle(holder, new Vector2Int(x, y));
    }

    private void RepaintWithDepth(LevelHolder holder, Vector2Int blockPos, byte depth)
    {
        var decision = new PlacingDecision();
        
        if (depth < BG_MidStart)
            decision.BG = BG_Outer;
        else if (depth < BG_InnerStart)
            decision.BG = BG_Mid;
        else
            decision.BG = BG_Inner;

        var mapSize = holder.GetMapSize();
        var matchesRight = blockPos.x + 1 < mapSize.x && holder.GetBlockTypeAt(blockPos.x + 1, blockPos.y) != BlockType.Dirt;
        var matchesLeft  = blockPos.x - 1 > 0         && holder.GetBlockTypeAt(blockPos.x - 1, blockPos.y) != BlockType.Dirt;
        var matchesAbove = blockPos.y + 1 < mapSize.y && holder.GetBlockTypeAt(blockPos.x, blockPos.y + 1) != BlockType.Dirt;
        var matchesBelow = blockPos.y - 1 > 0         && holder.GetBlockTypeAt(blockPos.x, blockPos.y - 1) != BlockType.Dirt;
        
        byte spriteIndex = 0;
        if (matchesRight) spriteIndex |= 1;
        if (matchesLeft) spriteIndex |= 2;
        if (matchesAbove) spriteIndex |= 4;
        if (matchesBelow) spriteIndex |= 8;

        ambiguityValue = (byte)Random.Range(0, 1000);
        decision.Matching = MatchingGetters[spriteIndex]();
        
        holder.ChangeVisualAt(blockPos, decision);
    }

    private byte GetDepth(LevelHolder holder, Vector2Int pos)
    {
        var mapSize = holder.GetMapSize();
        
        for (byte layer = 1; layer < BG_InnerStart; layer++)
        {
            var minX = Math.Max(pos.x - layer, 0);
            var maxX = Math.Min(pos.x + layer, mapSize.x - 1);
            var minY = Math.Max(pos.y - layer, 0);
            var maxY = Math.Min(pos.y + layer, mapSize.y - 1);
            
            for (int x = minX; x < maxX; x++)
                if (holder.GetBlockTypeAt(x, maxY) == BlockType.None
                    || holder.GetBlockTypeAt(x + 1, minY) == BlockType.None)
                    return layer;
            for (int y = minY; y < maxY; y++)
                if (holder.GetBlockTypeAt(minX, y) == BlockType.None
                    || holder.GetBlockTypeAt(maxX, y + 1) == BlockType.None)
                    return layer;
        }

        return BG_InnerStart;
    }
}
