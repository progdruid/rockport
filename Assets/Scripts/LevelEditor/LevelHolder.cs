using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class LevelHolder : MonoBehaviour
{
    [SerializeField] private Vector2Int mapSize;
    [Space] 
    [SerializeField] private Tilemap groundMap;
    [SerializeField] private Tilemap matchingMap;
    [SerializeField] private Grid visualGrid;
    [Space]
    [SerializeField] private BlockPlacingScript dirtPlacingScript;
    
    
    private BlockData[,] blockMap;
    
    private void Start()
    {
        blockMap = new BlockData[mapSize.x, mapSize.y];
    }
    
    public void ChangeBlockAt(Vector2 pos, BlockType newType)
    {
        var blockPos = Vector2Int.FloorToInt(pos / visualGrid.cellSize) + mapSize / 2;
        
        ref var block = ref blockMap[blockPos.x, blockPos.y]; ;
        if (block.type == newType)
            return;
        
        block.pos = blockPos;
        block.type = newType;
        block.collisionAmbiguityValue = 0;
        block.fillingAmbiguityValue = 0;

        if (newType == BlockType.Dirt)
        {
            dirtPlacingScript.RepaintSingle(this, blockPos);
            dirtPlacingScript.RepaintEnvironment(this, blockPos);
        }
        else if (newType == BlockType.None)
        {
            ChangeVisualAt(blockPos, new PlacingDecision());
            dirtPlacingScript.RepaintEnvironment(this, blockPos);
        }
    }

    public void ChangeVisualAt(Vector2Int blockPos, PlacingDecision decision)
    {
        var visualPos = (Vector3Int)(blockPos - mapSize / 2);
        groundMap.SetTile(visualPos, decision.BG);
        matchingMap.SetTile(visualPos, decision.Matching);
    }

    public Vector3 GetOrigin()
    {
        return groundMap.gameObject.transform.position;
    }
    
    public BlockType GetBlockTypeAt(int x, int y) => blockMap[x, y].type;
    public Vector2Int GetMapSize() => mapSize;
}
