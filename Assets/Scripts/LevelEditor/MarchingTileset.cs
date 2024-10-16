using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace LevelEditor
{
    public struct MarchingTileQuery
    {
        public bool[] Neighbours;
        
        public override int GetHashCode()
        {
            var hash = new HashCode();
            if (Neighbours == null) return hash.ToHashCode();
            foreach (var n in Neighbours) hash.Add(n);
            return hash.ToHashCode();
        }

        public override bool Equals(object obj) => obj is MarchingTileQuery other && Equals(other);
        public bool Equals(MarchingTileQuery other) => 
            Neighbours == other.Neighbours || 
            (Neighbours != null && other.Neighbours != null &&
             Neighbours.Length == other.Neighbours.Length &&
             Neighbours.SequenceEqual(other.Neighbours));
    }
    
    [CreateAssetMenu(menuName = "Polymort/Marching Tileset")]
    public class MarchingTileset : ScriptableObject
    {
        [SerializeField] private int TileSize;
        [SerializeField] private Texture2D TilesetTexture;
        [SerializeField] private int PPU;
        
        private Dictionary<MarchingTileQuery, List<TileBase>> _tiles;

        public bool TryGetTile(MarchingTileQuery query, out TileBase[] tiles)
        {
            if (_tiles == null || _tiles.Count == 0)
                ParseTiles();
            
            var found = _tiles.TryGetValue(query, out var list);
            tiles = list?.ToArray();
            return found;
        }

        public void ParseTiles()
        {
            if (_tiles == null)
                _tiles = new();
            else
                _tiles.Clear();
            
            var widthInTiles = TilesetTexture.width / TileSize;
            var heightInTiles = TilesetTexture.height / TileSize;

            //ceiling
            var reservedWidthInTiles = (widthInTiles + TileSize - 1) / TileSize;
            var reservedHeightInTiles = (heightInTiles * 2 + TileSize - 1) / TileSize;
            
            var includeMap = new bool[widthInTiles, heightInTiles];
            var groundMap = new bool[widthInTiles, heightInTiles];
            
            var includeColors = TilesetTexture.GetPixels(0, 0, widthInTiles, heightInTiles);
            var groundColors = TilesetTexture.GetPixels(0, heightInTiles, widthInTiles, heightInTiles);
            for (var x = 0; x < widthInTiles; x++)
            for (var y = 0; y < heightInTiles; y++)
            {
                var i = widthInTiles * y + x;
                includeMap[x, y] = includeColors[i].a > 0.5f;
                groundMap[x, y] = groundColors[i].a > 0.5f;
            }
            
            for (var x = 0; x < widthInTiles; x++)
            for (var y = 0; y < heightInTiles; y++)
            {   
                if (!includeMap[x, y] || (x < reservedWidthInTiles && y < reservedHeightInTiles))
                    continue;

                var rect = new RectInt(x * TileSize, y * TileSize, TileSize, TileSize);
                var tex = new Texture2D(TileSize, TileSize);
                tex.SetPixels(TilesetTexture.GetPixels(rect.x, rect.y, rect.width, rect.height));
                tex.Apply();
                
                var tileSprite = Sprite.Create(tex, new Rect(0, 0, TileSize, TileSize), new Vector2(0.5f, 0.5f), PPU);
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = tileSprite;

                var query = new MarchingTileQuery();
                query.Neighbours = new bool[PolyUtil.NeighbourOffsets.Length];
                for (var i = 0; i < PolyUtil.NeighbourOffsets.Length; i++)
                {
                    var n = new Vector2Int(x, y) + PolyUtil.NeighbourOffsets[i];
                    query.Neighbours[i] = n.x >= 0 && n.x < widthInTiles && n.y >= 0 && n.y < heightInTiles &&
                                          groundMap.At(n);
                }
                
                if (!_tiles.ContainsKey(query))
                    _tiles.Add(query, new());
                _tiles[query].Add(tile);
            }
        }
    }


// #if UNITY_EDITOR
//     
//     [CustomEditor(typeof(MarchingTileset))]
//     public class MarchingTilesetEditor : Editor
//     {
//         public override void OnInspectorGUI()
//         {
//             // Draw the default inspector first
//             DrawDefaultInspector();
//
//             // Get a reference to the target object (MarchingTileset)
//             var tileset = (MarchingTileset)target;
//
//             // Add a button that calls ParseTiles when clicked
//             if (!GUILayout.Button("Parse Tiles")) return;
//
//             tileset.ParseTiles();
//             Debug.Log("Tiles parsed successfully.");
//         }
//     }
//     
// #endif
}