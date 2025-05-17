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

    [HideInInspector] [SerializeField] private RectTransform mainRect;
    [HideInInspector] [SerializeField] private RectTransform borderRect;
    [HideInInspector] [SerializeField] private RectTransform shadowRect;
    [HideInInspector] [SerializeField] private RectTransform container;
    
    private RectTransform _rect;
    
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Assert.IsNotNull(sprite);

        for (var i = _rect.childCount - 1; i >= 0; i--)
            Destroy(_rect.GetChild(i).gameObject);

        ReloadLayers(_rect);
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void ReloadLayers(RectTransform rect)
    {
        if (container)
            container.SetParent(_rect);
        
        if (shadowRect)
        {
            shadowRect.SetParent(null);
            DestroyImmediate(shadowRect.gameObject);
            shadowRect = null;
        }
        if (borderRect)
        {
            borderRect.SetParent(null);
            DestroyImmediate(borderRect.gameObject);
            borderRect = null;
        }
        if (mainRect)
        {
            mainRect.SetParent(null);
            DestroyImmediate(mainRect.gameObject);
            mainRect = null;
        }
        
        shadowRect = CreateLayer("Shadow", rect, 0f, shadowOffset, shadowColor, 0, cornerShadow);
        borderRect = CreateLayer("Border", rect, 0f, Vector2.zero, borderColor, 1, cornerBorder);
        mainRect = CreateLayer("Main", rect, -borderThickness, Vector2.zero, mainColor, 2, cornerMain);
        
        if (!container)
            container = new GameObject("Container", typeof(RectTransform)).GetComponent<RectTransform>();
        container.SetParent(mainRect);
        container.anchorMin = Vector2.zero;
        container.anchorMax = Vector2.one;
        container.offsetMin = Vector2.zero;
        container.offsetMax = Vector2.zero;
        container.anchoredPosition = Vector2.zero;
    }

    private RectTransform CreateLayer(string name, Transform parent, float borderSize, Vector2 offset, Color color, int order,
        float cornerMultiplier)
    {
        var obj = new GameObject(name, typeof(Image));
        obj.transform.SetParent(parent, false);
        obj.hideFlags = HideFlags.NotEditable;
        obj.transform.SetSiblingIndex(order);

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

        return rect;
    }
    
#if UNITY_EDITOR
    //editor////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private bool _delayedLoading = false;
    
    private void OnValidate()
    {
        if (!sprite)
        {
            Debug.LogWarning("BrutalPanel: No sprite assigned. Please assign a sprite to the RoundedPanel component.");
            return;
        }
        
        if (_delayedLoading) return;
        
        _delayedLoading = true;
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (!this)
            {
                _delayedLoading = false;
                return;
            }
            
            var rect = GetComponent<RectTransform>();
            
            ReloadLayers(rect);
            
            _delayedLoading = false;
        };
    }
    
#endif
}

}