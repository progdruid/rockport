using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace LevelEditor
{
    [System.Serializable]
    public struct DirtLayer
    {
        [SerializeField] public int thickness;
        
        [Space] 
        [SerializeField] public TileBase baseTile;
        
        [Space] [Range(0f, 1f)] 
        [SerializeField] public float lowerPebbleDensity;

        [SerializeField] public TileBase[] lowerPebbles;
        
        [Range(0f, 1f)] 
        [SerializeField] public float upperPebbleDensity;
        [SerializeField] public TileBase[] upperPebbles;

        [SerializeField] public TileMarchingSet marchingSet;
    }

    public class DirtManipulator : MonoBehaviour
    {
        [SerializeField] private LevelSpaceHolder holder;
        [SerializeField] private int maxDepth;
        [SerializeField] private TileMarchingSet outlineMarchingSet;
        [SerializeField] private DirtLayer[] layers;
        
        private int[,] _depthMap;
        private Tilemap _baseMap;
        private Tilemap _lowerPebbleMap;
        private Tilemap _upperPebbleMap;
        private Tilemap _marchingMap;


        #region Getters and Setters

        public float GetZForInteraction() => _baseMap.transform.position.z;

        #endregion


        #region Private Logic

        private void Awake()
        {
            Assert.IsNotNull(holder);
            Assert.IsNotNull(outlineMarchingSet);
            Assert.IsNotNull(layers);

            outlineMarchingSet.ParseTiles();
            
            foreach (var layer in layers)
                if (layer.marchingSet)
                    layer.marchingSet.ParseTiles();
        }

        private void Start()
        {
            _depthMap = new int[holder.Size.x, holder.Size.y];
            
            _baseMap = holder.CreateTilemap(0, 0, "Dirt Base Tilemap");
            _baseMap.gameObject.AddComponent<TilemapRenderer>();
            
            _lowerPebbleMap = holder.CreateTilemap(0, 1, "Dirt Lower Pebble Tilemap");
            _lowerPebbleMap.gameObject.AddComponent<TilemapRenderer>();
            
            _upperPebbleMap = holder.CreateTilemap(0, 2, "Dirt Upper Pebble Tilemap");
            _upperPebbleMap.gameObject.AddComponent<TilemapRenderer>();
            
            _marchingMap = holder.CreateTilemap(0, 3, "Dirt Marching Tilemap");
            _marchingMap.gameObject.AddComponent<TilemapRenderer>();
        }

        private int RetrieveMinNeighbourDepth(Vector2Int pos)
        {
            var minDepth = maxDepth;
            foreach (var neighbour in holder.RetrievePositions(pos, PolyUtil.FullNeighbourOffsets))
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
                foreach (var current in layers)
                {   
                    var currentLayerEndDepth = lastLayerEndDepth + current.thickness;
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
                var inBounds = holder.IsInBounds(n);
                var present = inBounds && _depthMap.At(n) > lastLayerEndDepth;
                var check = (!inBounds && depth != 0) || present;
                fullQuery.Neighbours[i] = check;
                if (i < PolyUtil.HalfNeighbourOffsets.Length)
                    halfQuery.Neighbours[i] = check;
            }

            //march
            var marchingSet = depth != 0 ? foundLayer?.marchingSet : outlineMarchingSet;
            var marchingTile
                = (marchingSet &&
                   (marchingSet.TryGetTile(fullQuery, out var variants) ||
                    marchingSet.TryGetTile(halfQuery, out variants)))
                    ? variants[UnityEngine.Random.Range(0, variants.Length)]
                    : null;
            _marchingMap.SetTile((Vector3Int)pos, marchingTile);
            
            
            if (foundLayer == null)
            {
                _baseMap.SetTile((Vector3Int)pos, null);
                _lowerPebbleMap.SetTile((Vector3Int)pos, null);
                _upperPebbleMap.SetTile((Vector3Int)pos, null);
                return;
            }
            
            var layer = foundLayer.Value;

            // base
            _baseMap.SetTile((Vector3Int)pos, layer.baseTile);

            //pebbles
            Random.InitState(pos.x * 100 + pos.y);
            var rndLower = Random.Range(0, 10000);
            var shouldPlaceLower = rndLower <= layer.lowerPebbleDensity * 10000f;
            var lowerPebbles = layer.lowerPebbles;
            var lowerPebble = (shouldPlaceLower && lowerPebbles?.Length > 0)
                ? lowerPebbles[rndLower % lowerPebbles.Length]
                : null;
            _lowerPebbleMap.SetTile((Vector3Int)pos, lowerPebble);


            Random.InitState(rndLower);
            var rndUpper = Random.Range(0, 10000);
            var shouldPlaceUpper = rndUpper <= layer.upperPebbleDensity * 10000f;
            var upperPebbles = layer.upperPebbles;
            var upperPebble = (shouldPlaceUpper && upperPebbles?.Length > 0)
                ? upperPebbles[rndUpper % upperPebbles.Length]
                : null;
            _upperPebbleMap.SetTile((Vector3Int)pos, upperPebble);
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
                foreach (var neighbour in holder.RetrievePositions(pos, PolyUtil.FullNeighbourOffsets))
                {
                    var currentDepth = _depthMap.At(neighbour);
                    var calculatedDepth = Mathf.Min(RetrieveMinNeighbourDepth(neighbour) + 1, maxDepth);
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
            var inBounds = holder.ConvertWorldToMap(worldPos, out var mapPos);
            if (!inBounds) return;

            ChangeDepthAt(mapPos, place);
        }

        #endregion
    }
}