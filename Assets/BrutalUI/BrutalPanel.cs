using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BrutalUI
{

[RequireComponent(typeof(RectTransform))]
public class BrutalPanel : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Styling")] [SerializeField] private Color mainColor = Color.white;
    [SerializeField] private Color borderColor = Color.black;
    [SerializeField] private Color shadowColor = Color.black;
    [SerializeField] private float borderThickness = 8f;
    [SerializeField] private Vector2 shadowOffset = new(8, -8);

    [Header("Corner Rounding Per Layer")]
    [Tooltip("Separate rounding for each layer (Sliced sprite with border)")]
    [SerializeField] private float cornerMain = 1f;
    [SerializeField] private float cornerBorder = 1f;
    [SerializeField] private float cornerShadow = 1f;

    [Space] [SerializeField] private Sprite sprite;

    private RectTransform _rect;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Assert.IsNotNull(sprite);

        for (var i = _rect.childCount - 1; i >= 0; i--)
            Destroy(_rect.GetChild(i).gameObject);

        LoadLayers(_rect);
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void LoadLayers(RectTransform rect)
    {
        CreateLayer("Shadow", rect, 0f, shadowOffset, shadowColor, 0, cornerShadow);
        CreateLayer("Border", rect, 0f, Vector2.zero, borderColor, 1, cornerBorder);
        CreateLayer("Main", rect, -borderThickness, Vector2.zero, mainColor, 2, cornerMain);
    }

    private void CreateLayer(string name, Transform parent, float borderSize, Vector2 offset, Color color, int order,
        float cornerMultiplier)
    {
        var obj = new GameObject(name, typeof(Image));
        obj.transform.SetParent(parent, false);
        obj.hideFlags = HideFlags.NotEditable;

        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.sizeDelta += new Vector2(borderSize, borderSize) * 2f;
        rect.anchoredPosition = offset;


        var img = obj.GetComponent<Image>();

        img.sprite = sprite;
        img.type = Image.Type.Sliced;

        img.color = color;
        img.pixelsPerUnitMultiplier = cornerMultiplier;

        obj.transform.SetSiblingIndex(order);
    }
    
#if UNITY_EDITOR
    //editor////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private bool _delayedLoading = false;
    
    private void OnValidate()
    {
        if (!sprite)
        {
            Debug.LogWarning("BrutalButton: No sprite assigned. Please assign a sprite to the RoundedPanel component.");
            return;
        }
        
        if (_delayedLoading) return;
        
        _delayedLoading = true;
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (!this) return;
            
            var rect = GetComponent<RectTransform>();
            
            for (var i = rect.childCount - 1; i >= 0; i--)
                DestroyImmediate(rect.GetChild(i).gameObject);
            
            LoadLayers(rect);
            
            _delayedLoading = false;
        };
    }
    
#endif
}

}