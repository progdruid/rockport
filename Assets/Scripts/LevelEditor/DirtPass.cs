using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "", menuName = "Level Passes/Dirt Pass")]
public class DirtPass : LevelPass
{
    [SerializeField] private TileBase DirtOuter; 
    [SerializeField] private TileBase DirtMid; 
    [SerializeField] private TileBase DirtInner;
    [SerializeField] private int MaxDepth = 6;
    
    private Datamap _typeMap;
    private Tilemap _groundTilemap;

    private int[,] _depthMap;

    private Vector2Int _mapSize;
    
    public override void InjectLevelHolder(LevelHolder holder)
    {
        base.InjectLevelHolder(holder);

        _mapSize = _levelHolder.GetMapSize();
        _depthMap = new int[_mapSize.x, _mapSize.y];
        
        _typeMap = _levelHolder.ObtainDatamap<BlockType>("Type");
        _levelHolder.GetVisualMap("Ground", out _groundTilemap);
        
        _typeMap.ModificationEvent += OnTypeMapModified;
    }

    private void OnTypeMapModified(Vector2Int pos)
    {
        var blockType = _typeMap.At<BlockType>(pos);
        if (blockType != BlockType.Dirt)
            _depthMap[pos.x, pos.y] = 0;
        ComputeDepth(new () {pos});
    }

    private void ComputeDepth(HashSet<Vector2Int> posToChange)
    {
        HashSet<Vector2Int> changedPos = new();
        HashSet<Vector2Int> nextPos = new();
        
        while (posToChange.Any())
        {
            changedPos.AddRange(posToChange);
            
            foreach (var pos in posToChange)
            {
                //var pos = posToChange[i];
                ref var depth = ref _depthMap[pos.x, pos.y];
                
                Vector2Int min = new(Mathf.Max(pos.x-1, 0), Mathf.Max(pos.y-1, 0));
                Vector2Int max = new(Mathf.Min(pos.x+1,_mapSize.x-1), Mathf.Min(pos.y+1,_mapSize.y-1));

                List<Vector2Int> toAdd = new ();
                int minNeighbour = int.MaxValue;

                var lambdaDepth = depth;
                Action<Vector2Int> CheckFor = (checkPos) =>
                {
                    var checkDepth = _depthMap[checkPos.x, checkPos.y];
                    if (_typeMap.At<BlockType>(checkPos) == BlockType.Dirt 
                        && !nextPos.Contains(checkPos)/* && !changedPos.Contains(checkPos)*/)
                        toAdd.Add(checkPos);

                    if (checkDepth < minNeighbour)
                        minNeighbour = checkDepth;
                };
                
                CheckFor(min);
                CheckFor(new(pos.x, min.y));
                CheckFor(new(max.x, min.y));
                CheckFor(new(max.x, pos.y));
                CheckFor(max);
                CheckFor(new(pos.x, max.y));
                CheckFor(new(min.x, max.y));
                CheckFor(new(min.x, pos.y));
                
                if (Mathf.Min(minNeighbour + 1, MaxDepth) != depth && _typeMap.At<BlockType>(pos) == BlockType.Dirt)
                    depth = minNeighbour + 1;
                    
                UpdateVisualMapAt(pos, depth);
                
                nextPos.AddRange(toAdd);
            }
            
            (posToChange, nextPos) = (nextPos, posToChange);
            nextPos.Clear();
        }
        
    }
    

    private void UpdateVisualMapAt(Vector2Int pos, int depth)
    {
        var tile = depth switch
        {
            0 => null,
            1 => DirtOuter,
            2 => DirtMid,
            _ => DirtInner
        };
        _levelHolder.ChangeVisualAt(_groundTilemap, pos, tile);
    }
}
