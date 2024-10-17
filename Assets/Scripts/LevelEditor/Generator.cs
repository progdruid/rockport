using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace LevelEditor
{
    [Serializable]
    struct DirtLayer
    {
        public int StartingDepth;
        [Space]
        public TileBase Base;
        public MarchingTileset Marching;
        public TileBase[] LowerPebbles;
        public TileBase[] UpperPebbles;
    }

    public class Generator : MonoBehaviour
    {
        [SerializeField] private Vector2Int Size;
        [SerializeField] private int MaxDepth;
        [Space]
        [SerializeField] private Grid VisualGrid;
        [SerializeField] private Tilemap BaseMap;
        [FormerlySerializedAs("MatchingMap")] [SerializeField] private Tilemap MarchingMap;
        [SerializeField] private Tilemap LowerPebbleMap;
        [SerializeField] private Tilemap UpperPebbleMap;
        [Space] 
        [SerializeField] private DirtLayer OuterDirtLayer;
        [SerializeField] private DirtLayer MidDirtLayer;
        [SerializeField] private DirtLayer InnerDirtLayer;
    
        private int[,] _depthMap;
    
    
        #region Getters and Setters
    
        public float GetZ() => VisualGrid.transform.position.z;
    
        #endregion

    
        #region Private Logic

        private void Awake()
        {
            _depthMap = new int[Size.x, Size.y];
            
            OuterDirtLayer.Marching.ParseTiles();
            MidDirtLayer.Marching.ParseTiles();
            InnerDirtLayer.Marching.ParseTiles();
        }

        public bool ConvertWorldToMap(Vector2 worldPos, out Vector2Int mapPos)
        {
            var origin = VisualGrid.transform.position;
            mapPos = Vector2Int.FloorToInt((worldPos - (Vector2)origin) / VisualGrid.cellSize);
            return new Rect(0, 0, Size.x - 0.1f, Size.y - 0.1f).Contains(mapPos);
        }

        private IEnumerable<Vector2Int> RetrieveNeighbours(Vector2Int pos)
        {
            foreach(var direction in PolyUtil.FullNeighbourOffsets)
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
                var depth = _depthMap.At(neighbour);
                if (depth < minDepth) minDepth = depth;
            }
            return minDepth;
        }

        private void UpdateVisualAt(Vector2Int pos)
        {
            var depth = _depthMap.At(pos);
            
            DirtLayer? layer = null;
            if (depth >= InnerDirtLayer.StartingDepth)
                layer = InnerDirtLayer;
            else if (depth >= MidDirtLayer.StartingDepth)
                layer = MidDirtLayer;
            else if (depth == OuterDirtLayer.StartingDepth)
                layer = OuterDirtLayer;

            BaseMap.SetTile((Vector3Int)pos, layer?.Base);

            if (layer == null)
            {
                MarchingMap.SetTile((Vector3Int)pos, null);
                return;
            }
            
            var fullQuery = new MarchingTileQuery( new bool[PolyUtil.FullNeighbourOffsets.Length] );
            for (var i = 0; i < PolyUtil.FullNeighbourOffsets.Length; i++)
            {
                var n = pos + PolyUtil.FullNeighbourOffsets[i];
                fullQuery.Neighbours[i] = !PolyUtil.IsInBounds(n, Vector2Int.zero, Size) ||
                                          _depthMap.At(n) >= layer.Value.StartingDepth;
            }

            var marching = layer.Value.Marching;
            TileBase marchingTile = null;
            if (marching && 
                (marching.TryGetTile(fullQuery, out var variants) || 
                 marching.TryGetTile(new(fullQuery.Neighbours[..(fullQuery.Neighbours.Length / 2)]), out variants)))
            {
                marchingTile = variants[Random.Range(0, variants.Length)];
            }

            MarchingMap.SetTile((Vector3Int)pos, marchingTile);
        }
        
        private void ChangeDepthAt(Vector2Int rootPos, bool place)
        {
            var oldRootDepth = _depthMap.At(rootPos);
            if ((oldRootDepth == 0) != place)
                return;
        
            var pending = new Dictionary<Vector2Int, int>();
            pending[rootPos] = place ? 1 : 0;

            while (pending.Count > 0)
            {
                var pos = pending.Keys.Last();
                pending.TryGetValue(pos, out var depth);
                pending.Remove(pos);
            
                _depthMap.Set(pos, depth);
                foreach (var neighbour in RetrieveNeighbours(pos))
                {
                    var currentDepth = _depthMap.At(neighbour);
                    var calculatedDepth = Mathf.Min(RetrieveMinNeighbourDepth(neighbour) + 1, MaxDepth);
                    if (currentDepth == 0 || currentDepth == calculatedDepth)
                    {
                        UpdateVisualAt(neighbour); //this is temporary
                        continue;
                    }
                
                    pending[neighbour] = calculatedDepth;
                }

                UpdateVisualAt(pos);
            }
        
        }
    
        #endregion
    
    
        #region Public Interface

        public void ChangeTileAtWorldPos(Vector2 worldPos, bool place)
        {
            var inBounds = ConvertWorldToMap(worldPos, out var mapPos);
            if (!inBounds) return;
        
            ChangeDepthAt(mapPos, place);
        }
    
        #endregion

    }
}