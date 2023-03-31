using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BodySideTrigger : MonoBehaviour
{
    public bool triggered => trigPlayer != null || trigCorpses.Count > 0 || trigWalls.Count > 0;
    public bool playerTriggered => trigPlayer != null;
    public bool corpseTriggered => trigCorpses.Count > 0;
    public bool wallTriggered => trigWalls.Count > 0;

    public Rigidbody2D playerRB { get; private set; }
    public Rigidbody2D corpseRB { get; private set; }

    private Collider2D trigPlayer = null;
    private List<Collider2D> trigCorpses = new List<Collider2D>();
    private List<Collider2D> trigWalls = new List<Collider2D>();

    public event System.Action<Collider2D> EnterEvent = delegate { };
    public event System.Action<Collider2D> ExitEvent = delegate { };

    private void OnTriggerEnter2D (Collider2D other)
    {
        if (other.tag == "Player" && trigPlayer == null)
        {
            trigPlayer = other;
            playerRB = other.GetComponent<Rigidbody2D>();
        }
        else if (other.tag == "Corpse" && !trigCorpses.Contains(other))
        {
            trigCorpses.Add(other);
            corpseRB = other.GetComponent<Rigidbody2D>();
        }
        else if (other.tag != "Player" && other.tag != "Corpse" && !trigWalls.Contains(other))
            trigWalls.Add(other);
        
        EnterEvent(other);
        //Debug.Log($"{gameObject.name}: p = {playerTriggered}, c = {corpseTriggered}, w = {wallTriggered}.");
    }

    private void OnTriggerExit2D (Collider2D other)
    {
        if (other.tag == "Player")
        {
            trigPlayer = null;
            playerRB = null;
        }
        else if (other.tag == "Corpse" && trigCorpses.Contains(other))
        {
            trigCorpses.Remove(other);
            if (corpseRB != null && corpseRB.gameObject == other.gameObject)
                corpseRB = null;
        }
        else if (other.tag != "Player" && other.tag != "Corpse" && trigWalls.Contains(other))
            trigWalls.Remove(other);

        ExitEvent(other);
        //Debug.Log($"{gameObject.name}: p = {playerTriggered}, c = {corpseTriggered}, w = {wallTriggered}.");
    }
}
