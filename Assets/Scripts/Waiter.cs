using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waiter
{

    public static void Delay (float seconds)
    {
        float estimatedTime = Time.time + seconds;
        while (Time.time < estimatedTime);
    }
}
