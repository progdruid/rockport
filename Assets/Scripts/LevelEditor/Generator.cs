
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class Generator : MonoBehaviour
{
    [SerializeField] private Vector2Int Size;
    [Space]
    [SerializeField] private TileBase DirtTile;
    [Space]
    [SerializeField] private Grid VisualGrid;
    [SerializeField] private Tilemap BaseMap;
    [SerializeField] private Tilemap MatchingMap;
    [SerializeField] private Tilemap LowerPebbleMap;
    [SerializeField] private Tilemap UpperPebbleMap;

    private int[,] _map;

    public float GetZ() => VisualGrid.transform.position.z;
    
    public void PlaceDirtAt(Vector2 worldPos)
    {
        var mapPos = ConvertWorldToMap(worldPos);
        BaseMap.SetTile(mapPos, DirtTile);
    }

    public void CarveDirtAt(Vector2 worldPos)
    {
        var mapPos = ConvertWorldToMap(worldPos);
        BaseMap.SetTile(mapPos, null);
    }

    private Vector3Int ConvertWorldToMap(Vector2 worldPos)
    {
        var origin = VisualGrid.transform.position;
        var mapPos = Vector2Int.FloorToInt((worldPos - (Vector2)origin) / VisualGrid.cellSize);
        return new Vector3Int(mapPos.x, mapPos.y, Mathf.FloorToInt(origin.z));
    }
    
    private void Awake()
    {
        _map = new int[Size.x, Size.y];
    }

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