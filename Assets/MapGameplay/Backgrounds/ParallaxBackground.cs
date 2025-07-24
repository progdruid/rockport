using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ParallaxBackground : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] [Range(-1f, 1f)] private float depth;
    [SerializeField] private Sprite undergroundSprite;
    [SerializeField] private Sprite groundSprite;
    [SerializeField] private Sprite upperSprite;
    [SerializeField] private Material spriteMaterial;
    
    private Vector2 _halfSize;
    private Vector2? _groundAnchor;
    private Transform _target;

    private SpriteRenderer[,] _tiles;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        
        _halfSize = undergroundSprite.bounds.extents.To2();
        
        _tiles = new SpriteRenderer[3, 3];
        for (var x = -1; x <= 1; x++)
        for (var y = -1; y <= 1; y++)
        {
            var tile = new GameObject("tile").AddComponent<SpriteRenderer>();
            tile.transform.SetParent(transform);
            tile.transform.localPosition = new Vector3(x * 2f * _halfSize.x, y * 2f * _halfSize.y, 0f);
            tile.material = spriteMaterial;
            _tiles[x + 1, y + 1] = tile;
        }
    }

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetTarget(Transform target) => _target = target;
    public void SetGroundAnchor(Vector2 groundAnchor) => _groundAnchor = groundAnchor;
    public void Clear()
    {
        _target = null;
        _groundAnchor = null;
    }
    
    //game loop/////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void LateUpdate() => RunUpdate();

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void RunUpdate()
    {
        if (!_target) return;
        var usedGroundAnchor = _groundAnchor ?? Vector2.zero;

        var newPos = _target.position.To2() * depth + usedGroundAnchor * (1f - depth);
        var yOffsetOfTarget = _target.position.y - newPos.y;
        var yIndexCenter = ((yOffsetOfTarget + _halfSize.y) / (_halfSize.y * 2f)).FloorToInt();
        for (var y = -1; y <= 1; y++)
        {
            var yIndexRow = yIndexCenter + y;
            var usedSprite = yIndexRow switch
            {
                < 0 => undergroundSprite,
                0   => groundSprite,
                > 0 => upperSprite
            };
            for (var x = -1; x <= 1; x++) 
                _tiles[x + 1, y + 1].sprite = usedSprite;
        }
        newPos.x = (newPos.x - _target.position.x + _halfSize.x).Mod(_halfSize.x * 2) + _target.position.x - _halfSize.x;
        newPos.y = (newPos.y - _target.position.y + _halfSize.y).Mod(_halfSize.y * 2) + _target.position.y - _halfSize.y;
        
        transform.SetWorldXY(newPos);
    }
}
