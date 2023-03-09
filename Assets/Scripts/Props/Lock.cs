using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Lock : MonoBehaviour
{
    [SerializeField] SignalActivator signal;
    [SerializeField] TileBase tile1, tile2;

    private Tilemap tilemap;
    private Collider2D col;

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
        col = GetComponent<Collider2D>();
        
        if (signal != null)
            signal.ActivationUpdateEvent += HandleActivation;
    }

    private void OnDestroy()
    {
        if (signal != null)
            signal.ActivationUpdateEvent -= HandleActivation;
    }

    private void HandleActivation (bool activated, GameObject source)
    {
        col.enabled = !activated;
        if (activated)
            tilemap.SwapTile(tile1, tile2);
        else
            tilemap.SwapTile(tile2, tile1);
    }
}
