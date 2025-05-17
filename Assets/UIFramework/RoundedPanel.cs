using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class RoundedPanel : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Styling")]
    [SerializeField] private Color mainColor = Color.white;
    [SerializeField] private Color borderColor = Color.black;
    [SerializeField] private Color shadowColor = Color.black;
    [SerializeField] private float borderThickness = 8f;
    [SerializeField] private Vector2 shadowOffset = new(8, -8);
    
    [Header("Corner Rounding Per Layer")]
    [Tooltip("Separate rounding for each layer (Sliced sprite with border)")]
    [SerializeField] private float cornerMain = 1f;
    [SerializeField] private float cornerBorder = 1f;
    [SerializeField] private float cornerShadow = 1f;
    
    [Space]
    [SerializeField] private Sprite sprite;
    
    private RectTransform _rect;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Assert.IsNotNull(sprite);
        
        LoadLayers();
    }
    
    private void OnValidate()
    {
        _rect = GetComponent<RectTransform>();
        if (!sprite)
        {
            Debug.LogWarning("RoundedPanel: No sprite assigned. Please assign a sprite to the RoundedPanel component.");
            return;
        }
        

#if UNITY_EDITOR
        EditorApplication.delayCall += () =>
        {
            if (!this || !_rect) return;
            
            for (var i = _rect.childCount - 1; i >= 0; i--)
                DestroyImmediate(_rect.GetChild(i).gameObject);

            LoadLayers();
        };
#endif
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void LoadLayers()
    {
        CreateLayer("Shadow", _rect, 0f, shadowOffset, shadowColor, 0, cornerShadow);
        CreateLayer("Border", _rect, borderThickness, Vector2.zero, borderColor, 1, cornerBorder);
        CreateLayer("Main", _rect, 0f, Vector2.zero, mainColor, 2, cornerMain);
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
}
