
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class Generator : MonoBehaviour
{
    private static readonly Vector2Int[] Directions =
    {
        new(-1, 1 ), new( 0, 1 ), new( 1, 1 ), 
        new(-1, 0 ),    /*0, 0*/  new( 1, 0 ),
        new(-1,-1 ), new( 0,-1 ), new( 1,-1 ) 
    };
    
    [SerializeField] private Vector2Int Size;
    [SerializeField] private int MaxDepth;
    [Space]
    [SerializeField] private TileBase OuterDirtTile;
    [SerializeField] private TileBase MidDirtTile;
    [SerializeField] private TileBase InnerDirtTile;
    [Space]
    [SerializeField] private Grid VisualGrid;
    [SerializeField] private Tilemap BaseMap;
    [SerializeField] private Tilemap MatchingMap;
    [SerializeField] private Tilemap LowerPebbleMap;
    [SerializeField] private Tilemap UpperPebbleMap;

    private int[,] _map;
    
    
    #region Getters and Setters
    
    public float GetZ() => VisualGrid.transform.position.z;
    
    #endregion

    
    #region Private Logic

    private void Awake()
    {
        _map = new int[Size.x, Size.y];
    }

    public bool ConvertWorldToMap(Vector2 worldPos, out Vector2Int mapPos)
    {
        var origin = VisualGrid.transform.position;
        mapPos = Vector2Int.FloorToInt((worldPos - (Vector2)origin) / VisualGrid.cellSize);
        return new Rect(0, 0, Size.x - 0.1f, Size.y - 0.1f).Contains(mapPos);
    }

    private IEnumerable<Vector2Int> RetrieveNeighbours(Vector2Int pos)
    {
        foreach(var direction in Directions)
        {
            var neighbour = pos + direction;
            if (neighbour.x >= 0 && neighbour.x < Size.x && neighbour.y >= 0 && neighbour.y < Size.y)
                yield return neighbour;
        }
    }
    
    private int RetrieveMinNeighbourDepth(Vector2Int pos)
    {
        var minDepth = MaxDepth;
        foreach (var neighbour in RetrieveNeighbours(pos)) 
        {
            var depth = _map.At(neighbour);
            if (depth < minDepth) minDepth = depth;
        }
        return minDepth;
    }
    
    private void ChangeTileAt(Vector2Int rootPos, bool place)
    {
        var oldRootDepth = _map.At(rootPos);
        if ((oldRootDepth == 0) != place)
            return;
        
        var updateMap = new Dictionary<Vector2Int, int>();
        DoUpdate(rootPos, place ? 1 : 0);

        while (updateMap.Count > 0)
        {
            var pos = updateMap.Keys.Last();
            updateMap.TryGetValue(pos, out var newDepth);
            updateMap.Remove(pos);
            DoUpdate(pos, newDepth);
        }
        return;

        void DoUpdate(Vector2Int pos, int depth)
        {
            _map.Set(pos, depth);
            BaseMap.SetTile((Vector3Int)pos, depth switch 
            {
                >= 4 => InnerDirtTile,
                >= 2 => MidDirtTile,
                1 => OuterDirtTile, 
                _ => null // 0 + other
            });
            
            foreach (var neighbour in RetrieveNeighbours(pos))
            {
                var oldDepth = _map.At(neighbour);
                var newDepth = Mathf.Min(RetrieveMinNeighbourDepth(neighbour) + 1, MaxDepth);
                if (oldDepth == 0 || oldDepth == newDepth) 
                    continue;
                
                updateMap[neighbour] = newDepth;
            }
        }
    }
    
    #endregion
    
    
    #region Public Interface

    public void ChangeTileAtWorldPos(Vector2 worldPos, bool place)
    {
        var inBounds = ConvertWorldToMap(worldPos, out var mapPos);
        if (!inBounds) return;
        
        ChangeTileAt(mapPos, place);
    }
    
    #endregion

    // private void ComputeDepth(HashSet<Vector2Int> posToChange)
    // {
    //     HashSet<Vector2Int> changedPos = new();
    //     HashSet<Vector2Int> nextPos = new();
    //     
    //     while (posToChange.Any())
    //     {
    //         changedPos.AddRange(posToChange);
    //         
    //         foreach (var pos in posToChange)
    //         {
    //             //var pos = posToChange[i];
    //             ref var depth = ref _depthMap[pos.x, pos.y];
    //             
    //             Vector2Int min = new(Mathf.Max(pos.x-1, 0), Mathf.Max(pos.y-1, 0));
    //             Vector2Int max = new(Mathf.Min(pos.x+1,_mapSize.x-1), Mathf.Min(pos.y+1,_mapSize.y-1));
    //
    //             List<Vector2Int> toAdd = new ();
    //             int minNeighbour = int.MaxValue;
    //
    //             var lambdaDepth = depth;
    //             Action<Vector2Int> CheckFor = (checkPos) =>
    //             {
    //                 var checkDepth = _depthMap[checkPos.x, checkPos.y];
    //                 if (_typeMap.GetAt<BlockType>(checkPos) == BlockType.Dirt 
    //                     && !nextPos.Contains(checkPos) && !changedPos.Contains(checkPos))
    //                     toAdd.Add(checkPos);
    //
    //                 if (checkDepth < minNeighbour)
    //                     minNeighbour = checkDepth;
    //             };
    //             
    //             CheckFor(min);
    //             CheckFor(new(pos.x, min.y));
    //             CheckFor(new(max.x, min.y));
    //             CheckFor(new(max.x, pos.y));
    //             CheckFor(max);
    //             CheckFor(new(pos.x, max.y));
    //             CheckFor(new(min.x, max.y));
    //             CheckFor(new(min.x, pos.y));
    //             
    //             if (Mathf.Min(minNeighbour + 1, MaxDepth) == depth || _typeMap.GetAt<BlockType>(pos) != BlockType.Dirt)
    //                 continue;
    //             
    //             depth = minNeighbour + 1;
    //             UpdateVisualMapAt(pos, depth);
    //             
    //             nextPos.AddRange(toAdd);
    //         }
    //         
    //         (posToChange, nextPos) = (nextPos, posToChange);
    //         nextPos.Clear();
    //     }
    //     
    // }
}