using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum TriggeredType
{
    None,
    Dirt,
    Wood,
    Player,
    Corpse
}

[RequireComponent(typeof(Collider2D))]
public sealed class UniversalTrigger : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private readonly Dictionary<string, TriggeredType> _tagTypeMap = new()
    {
        { "Dirt", TriggeredType.Dirt },
        { "Wood", TriggeredType.Wood },
        { "Untagged", TriggeredType.Wood },
        { "Ground", TriggeredType.Wood },
        { "Player", TriggeredType.Player },
        { "Corpse", TriggeredType.Corpse }
    };
    
    private Collider2D _collider;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        Assert.IsNotNull(_collider); //no real need for this, as it's a required component
        
        _collider.isTrigger = true;
    }


    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public event System.Action<Collider2D, TriggeredType> EnterEvent;
    public event System.Action<Collider2D, TriggeredType> ExitEvent;

    //game loop/////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void OnTriggerEnter2D(Collider2D other) => EnterEvent?.Invoke(other, _tagTypeMap[other.tag]);
    private void OnTriggerExit2D(Collider2D other) => ExitEvent?.Invoke(other, _tagTypeMap[other.tag]);
}