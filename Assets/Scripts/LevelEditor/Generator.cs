using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace LevelEditor
{
    [System.Serializable]
    public struct DirtLayer
    {
        [SerializeField] public int Thickness;
        [Space] [SerializeField] public TileBase Base;

        [Space] [Range(0f, 1f)] [SerializeField]
        public float LowerPebbleDensity;

        [SerializeField] public TileBase[] LowerPebbles;

        [Range(0f, 1f)] [SerializeField] public float UpperPebbleDensity;
        [SerializeField] public TileBase[] UpperPebbles;

        [SerializeField] public TileMarchingSet MarchingSet;
    }

    public class Generator : MonoBehaviour
    {
        [SerializeField] private Vector2Int Size;
        [SerializeField] private int MaxDepth;
        [Space] 
        [SerializeField] private Grid VisualGrid;
        [SerializeField] private Tilemap BaseMap;

        [FormerlySerializedAs("MatchingMap")] 
        [SerializeField] private Tilemap MarchingMap;

        [SerializeField] private Tilemap LowerPebbleMap;
        [SerializeField] private Tilemap UpperPebbleMap;
        [Space] 
        [SerializeField] private TileMarchingSet OutlineMarchingSet;
        [SerializeField] private DirtLayer[] Layers;
        
        private int[,] _depthMap;


        #region Getters and Setters

        public float GetZ() => VisualGrid.transform.position.z;

        #endregion


        #region Private Logic

        private void Awake()
        {
            _depthMap = new int[Size.x, Size.y];

            OutlineMarchingSet.ParseTiles();
            ;
            foreach (var layer in Layers)
                if (layer.MarchingSet)
                    layer.MarchingSet.ParseTiles();
        }

        public bool ConvertWorldToMap(Vector2 worldPos, out Vector2Int mapPos)
        {
            var origin = VisualGrid.transform.position;
            mapPos = Vector2Int.FloorToInt((worldPos - (Vector2)origin) / VisualGrid.cellSize);
            return new Rect(0, 0, Size.x - 0.1f, Size.y - 0.1f).Contains(mapPos);
        }

        private IEnumerable<Vector2Int> RetrieveNeighbours(Vector2Int pos)
        {
            foreach (var direction in PolyUtil.FullNeighbourOffsets)
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

            //layer determining
            DirtLayer? foundLayer = null;
            var lastLayerEndDepth = 0;
            if (depth != 0)
                foreach (var current in Layers)
                {   
                    var currentLayerEndDepth = lastLayerEndDepth + current.Thickness;
                    if (depth <= currentLayerEndDepth)
                    {
                        foundLayer = current;
                        break;
                    }
                    lastLayerEndDepth = currentLayerEndDepth;
                }


            // marching query
            var fullQuery = new MarchingTileQuery(new bool[PolyUtil.FullNeighbourOffsets.Length]);
            var halfQuery = new MarchingTileQuery(new bool[PolyUtil.HalfNeighbourOffsets.Length]);
            for (var i = 0; i < PolyUtil.FullNeighbourOffsets.Length; i++)
            {
                var n = pos + PolyUtil.FullNeighbourOffsets[i];
                var inBounds = PolyUtil.IsInBounds(n, Vector2Int.zero, Size);
                var present = inBounds && _depthMap.At(n) > lastLayerEndDepth;
                var check = (!inBounds && depth != 0) || present;
                fullQuery.Neighbours[i] = check;
                if (i < PolyUtil.HalfNeighbourOffsets.Length)
                    halfQuery.Neighbours[i] = check;
            }

            //march
            var marchingSet = depth != 0 ? foundLayer?.MarchingSet : OutlineMarchingSet;
            var marchingTile
                = (marchingSet &&
                   (marchingSet.TryGetTile(fullQuery, out var variants) ||
                    marchingSet.TryGetTile(halfQuery, out variants)))
                    ? variants[UnityEngine.Random.Range(0, variants.Length)]
                    : null;
            MarchingMap.SetTile((Vector3Int)pos, marchingTile);
            
            
            if (foundLayer == null)
            {
                BaseMap.SetTile((Vector3Int)pos, null);
                LowerPebbleMap.SetTile((Vector3Int)pos, null);
                UpperPebbleMap.SetTile((Vector3Int)pos, null);
                return;
            }
            
            var layer = foundLayer.Value;

            // base
            BaseMap.SetTile((Vector3Int)pos, layer.Base);

            //pebbles
            Random.InitState(pos.x * 100 + pos.y);
            var rndLower = Random.Range(0, 10000);
            var shouldPlaceLower = rndLower <= layer.LowerPebbleDensity * 10000f;
            var lowerPebbles = layer.LowerPebbles;
            var lowerPebble = (shouldPlaceLower && lowerPebbles?.Length > 0)
                ? lowerPebbles[rndLower % lowerPebbles.Length]
                : null;
            LowerPebbleMap.SetTile((Vector3Int)pos, lowerPebble);


            Random.InitState(rndLower);
            var rndUpper = Random.Range(0, 10000);
            var shouldPlaceUpper = rndUpper <= layer.UpperPebbleDensity * 10000f;
            var upperPebbles = layer.UpperPebbles;
            var upperPebble = (shouldPlaceUpper && upperPebbles?.Length > 0)
                ? upperPebbles[rndUpper % upperPebbles.Length]
                : null;
            UpperPebbleMap.SetTile((Vector3Int)pos, upperPebble);
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