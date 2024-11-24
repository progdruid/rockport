using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class UIManipulatorPropertyEditor : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] GameObject propertyFieldPrefab;
    
    private void Awake()
    {
        Assert.IsNotNull(canvas);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void CreateField(string propertyName, float value)
    {
        var propertyObject = Instantiate(propertyFieldPrefab, canvas.transform);
        
    }
}
