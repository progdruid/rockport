
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[Serializable]
struct DirtLayer
{
    public int StartingDepth;
    [Space]
    public TileBase Base;
    [Space] 
    public TileBase MarchCornerNW;
    public TileBase MarchCornerNE;
    public TileBase MarchCornerSE;
    public TileBase MarchCornerSW;
    [Space] 
    public TileBase[] MarchEdgeN;
    public TileBase[] MarchEdgeE;
    public TileBase[] MarchEdgeS;
    public TileBase[] MarchEdgeW;
    [Space]
    public TileBase[] MarchBurgerNS;
    public TileBase[] MarchBurgerWE;
    [Space]
    public TileBase[] MarchEndN;
    public TileBase[] MarchEndE;
    public TileBase[] MarchEndS;
    public TileBase[] MarchEndW;
    [Space] 
    public TileBase MarchFull;
    [Space] 
    public TileBase[] LowerPebbles;
    public TileBase[] UpperPebbles;
}

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
    [SerializeField] private Grid VisualGrid;
    [SerializeField] private Tilemap BaseMap;
    [SerializeField] private Tilemap MatchingMap;
    [SerializeField] private Tilemap LowerPebbleMap;
    [SerializeField] private Tilemap UpperPebbleMap;
    [Space]
    [SerializeField] private TileBase OuterDirtTile;
    [SerializeField] private TileBase MidDirtTile;
    [SerializeField] private TileBase InnerDirtTile;
    [Space]
    
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
        
        var pending = new Dictionary<Vector2Int, int>();
        pending[rootPos] = place ? 1 : 0;

        while (pending.Count > 0)
        {
            var pos = pending.Keys.Last();
            pending.TryGetValue(pos, out var depth);
            pending.Remove(pos);
            
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
                var currentDepth = _map.At(neighbour);
                var calculatedDepth = Mathf.Min(RetrieveMinNeighbourDepth(neighbour) + 1, MaxDepth);
                if (currentDepth == 0 || currentDepth == calculatedDepth) 
                    continue;
                
                pending[neighbour] = calculatedDepth;
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