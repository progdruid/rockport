using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerousTriggerHandler : MonoBehaviour
{
    public void OnTriggerEnter2D ()
    {
        Debug.Log("Killed");
    }
}
