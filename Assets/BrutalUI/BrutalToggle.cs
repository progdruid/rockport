using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BrutalUI
{

public class BrutalToggle : MonoBehaviour, IPointerDownHandler
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Toggle")] 
    [SerializeField] private bool currentValue = false;
    [Range(0f, 1f)]
    [SerializeField] private float fillAmount = 0.6f;
    [SerializeField] private float toggleDuration = 0.1f;
    
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
    
    private RectTransform _rect;
    private Coroutine _toggleCoroutine = null;
    
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Assert.IsNotNull(sprite);
        
        ReloadLayers(_rect);
        UpdateState(_rect);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        currentValue = !currentValue;
        UpdateState(_rect);
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateState(RectTransform rect)
    {
        if (_toggleCoroutine != null)
            StopCoroutine(_toggleCoroutine);
        if (!gameObject.activeInHierarchy || !rect.gameObject.activeInHierarchy)
            return;
            
        _toggleCoroutine = StartCoroutine(UpdateStateRoutine(rect));
    }
    
    private IEnumerator UpdateStateRoutine(RectTransform rect)
    {
        var oldMin = mainRect.anchorMin;
        var oldMax = mainRect.anchorMax;
        var newMin = new Vector2(currentValue ? (1f - fillAmount) : 0f, 0f);
        var newMax = new Vector2(newMin.x + fillAmount, 1f);
        
        var elapsedTime = 0f;
        while (elapsedTime < toggleDuration)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / toggleDuration);
                
            var min = Vector2.Lerp(oldMin, newMin, t);
            var max = Vector2.Lerp(oldMax, newMax, t);
                
            mainRect.anchorMin = min;
            mainRect.anchorMax = max;
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;
        
            borderRect.anchorMin = min;
            borderRect.anchorMax = max;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
                
            mainRect.sizeDelta -= new Vector2(borderThickness, borderThickness) * 2f;
            
            yield return null;
        }
        
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
            UpdateState(rect);
            
            _delayedLoading = false;
        };
    }
    
#endif
}

}