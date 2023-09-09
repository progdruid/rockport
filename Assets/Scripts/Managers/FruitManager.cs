using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitManager : MonoBehaviour
{
    public event System.Action FruitUpdateEvent = delegate { };

    private int availableFruits;

    private void Awake() => Registry.ins.fruitManager = this;

    public void ClearFruits ()
    {
        availableFruits = 0;
        FruitUpdateEvent();
    }

    public void AddFruit ()
    {
        availableFruits++;
        FruitUpdateEvent();
    }

    public void DestroyFruit ()
    {
        availableFruits--;
        FruitUpdateEvent();
    }

    public int GetFruitsAmount() => availableFruits;
}
