using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ParallaxBackground : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] [Range(-1f, 1f)] float depth;
    [SerializeField] Sprite undergroundSprite;
    [SerializeField] Sprite groundSprite;
    [SerializeField] Sprite upperSprite;
    [SerializeField] Material spriteMaterial;
    
    private Vector2 _halfSize;
    private float? _groundLevel;
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
    public void SetGroundLevel(float groundLevel) => _groundLevel = groundLevel;
    public void Clear()
    {
        _target = null;
        _groundLevel = null;
    }
    
    //game loop/////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void LateUpdate() => RunUpdate();

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void RunUpdate()
    {
        if (!_target) return;
        var usedGroundLevel = _groundLevel ?? 0f;

        var newPos = _target.position.To2() * depth + usedGroundLevel * (1f - depth) * Vector2.up;
        var yOffsetOfTarget = _target.position.y - newPos.y;
        var yIndexCenter = ((yOffsetOfTarget + _halfSize.y) / (_halfSize.y * 2f)).FloorToInt();//- usedGroundLevel * (1f - depth)
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
