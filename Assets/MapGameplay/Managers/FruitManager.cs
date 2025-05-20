using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class FruitManager : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private TMP_Text fruitNumberText;
    
    private int _availableFruits;

    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(fruitNumberText);
        GameSystems.Ins.FruitManager = this;
        UpdateText();
    }
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public int GetFruitsAmount() => _availableFruits;
    public void ClearFruits ()
    {
        _availableFruits = 0;
        UpdateText();
    }

    public void AddFruit ()
    {
        _availableFruits++;
        UpdateText();
    }

    public void DestroyFruit ()
    {
        _availableFruits--;
        UpdateText();
    }


    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateText() => fruitNumberText.text = $"x{_availableFruits}";
}
