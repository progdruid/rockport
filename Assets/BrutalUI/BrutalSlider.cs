using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BrutalUI
{

[RequireComponent(typeof(RectTransform))]
public class BrutalSlider : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Slider")] 
    [Range(0f, 1f)]
    [SerializeField] private float currentValue = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float minFill;
    [SerializeField] private bool isVertical = false;
    
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

    [Space] [SerializeField] private Sprite sprite;

    [HideInInspector] [SerializeField] private RectTransform mainRect;
    [HideInInspector] [SerializeField] private RectTransform borderRect;
    
    private RectTransform _rect;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Assert.IsNotNull(sprite);
        
        ReloadLayers(_rect);
        UpdateWidth(_rect);
    }
    
    public void OnDrag(PointerEventData eventData) => UpdateValueFromPointer(eventData);
    public void OnPointerDown(PointerEventData eventData) => UpdateValueFromPointer(eventData);
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////

    private void UpdateValueFromPointer(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position,
            eventData.pressEventCamera, out var localPoint);

        var clip = isVertical
            ? localPoint.y / _rect.rect.height
            : localPoint.x / _rect.rect.width;

        var normalClip = Mathf.Clamp01(clip + 0.5f);
        currentValue = (normalClip - minFill) / (1f - minFill);
        UpdateWidth(_rect);
        //?.Invoke(currentValue);
    }
    
    private void UpdateWidth(RectTransform rect)
    {
        var newMax = isVertical
            ? new Vector2(1f, Mathf.Lerp(minFill, 1f, currentValue))
            : new Vector2(Mathf.Lerp(minFill, 1f, currentValue), 1f);
        
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = newMax;
        mainRect.offsetMin = Vector2.zero;
        mainRect.offsetMax = Vector2.zero;
        
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = newMax;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        
        mainRect.sizeDelta -= new Vector2(borderThickness, borderThickness) * 2f;
    }
    
    private void ReloadLayers(RectTransform rect)
    {
        for (var i = rect.childCount - 1; i >= 0; i--)
            DestroyImmediate(rect.GetChild(i).gameObject);
        
        CreateLayer("Shadow", rect, 0f, shadowOffset, shadowColor, 0, cornerShadow);
        borderRect = CreateLayer("Border", rect, 0f, Vector2.zero, borderColor, 1, cornerBorder);
        mainRect = CreateLayer("Main", rect, -borderThickness, Vector2.zero, mainColor, 2, cornerMain);
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
            Debug.LogWarning("BrutalSlider: No sprite assigned. Please assign a sprite to the RoundedPanel component.");
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
            UpdateWidth(rect);
            
            _delayedLoading = false;
        };
    }
    
#endif
    
}

}