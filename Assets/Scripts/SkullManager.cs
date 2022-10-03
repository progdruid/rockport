using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullManager : MonoBehaviour
{
    public event System.Action SkullUpdateEvent = delegate { };

    private int availableSkulls;

    private void Awake()
    {
        Registry.ins.skullManager = this;
    }

    public void ClearSkulls ()
    {
        availableSkulls = 0;
        SkullUpdateEvent();
    }

    public void AddSkull ()
    {
        availableSkulls++;
        SkullUpdateEvent();
    }

    public void DestroySkull ()
    {
        availableSkulls--;
        SkullUpdateEvent();
    }

    public int GetSkullsAmount() => availableSkulls;
}
