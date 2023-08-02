using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap), typeof(Collider2D))]
public class Lock : MonoBehaviour
{
    [System.Serializable]
    public struct TilePair {
        public TileBase shownTile, hiddenTile;
    }

    [SerializeField] SignalSource signal;
    [SerializeField] TilePair[] pairs;

    private Tilemap tilemap;
    private Collider2D col;

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
        col = GetComponent<Collider2D>();
        
        if (signal != null)
            signal.SignalUpdateEvent += HandleActivation;
    }

    private void OnDestroy()
    {
        if (signal != null)
            signal.SignalUpdateEvent -= HandleActivation;
    }

    private void HandleActivation (bool activated, GameObject source)
    {
        col.enabled = !activated;
        for (int i = 0; i < pairs.Length; i++)
        {
            TileBase tileToBeSwapped = null;
            TileBase tileToBeSwappedTo = null;
            if (activated)
            {
                tileToBeSwapped = pairs[i].shownTile;
                tileToBeSwappedTo = pairs[i].hiddenTile;
            }
            else
            {
                tileToBeSwapped = pairs[i].hiddenTile;
                tileToBeSwappedTo = pairs[i].shownTile;
            }
            tilemap.SwapTile(tileToBeSwapped, tileToBeSwappedTo);
        }
    }
}
