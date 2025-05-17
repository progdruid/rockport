using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BrutalUI
{

[RequireComponent(typeof(RectTransform))]
public class BrutalButton : MonoBehaviour, IPointerDownHandler
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Styling")] 
    [SerializeField] private Color mainColor = Color.white;
    [SerializeField] private Color pressedColor = Color.grey;
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

    [Header("Press")]
    [Range(0f, 1f)]
    [SerializeField] private float pressDepth = 0.95f;
    [SerializeField] private float pressDuration = 0.1f;
    [SerializeField] private UnityEvent pressEvent;
    
    [HideInInspector] [SerializeField] private RectTransform mainRect;
    [HideInInspector] [SerializeField] private RectTransform borderRect;
    [HideInInspector] [SerializeField] private RectTransform shadowRect;
    [HideInInspector] [SerializeField] private Image mainImage;
    [HideInInspector] [SerializeField] private RectTransform container;
    
    private RectTransform _rect = null;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Assert.IsNotNull(sprite);

        ReloadLayers();
    }
    
    public void OnPointerDown(PointerEventData eventData) => StartCoroutine(Press());
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator Press()
    {
        var elapsedTime = 0f;
        while (elapsedTime < pressDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / (pressDuration / 2);
        
            var pressAmount = Mathf.Lerp(0f, pressDepth, t);
            mainRect.anchoredPosition = shadowOffset * pressAmount;
            borderRect.anchoredPosition = shadowOffset * pressAmount;
        
            var color = Color.Lerp(mainColor, pressedColor, t);
            mainImage.color = color;
            
            yield return null;
        }
    
        pressEvent?.Invoke();
    
        elapsedTime = 0f;
        while (elapsedTime < pressDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / (pressDuration / 2);
        
            var pressAmount = Mathf.Lerp(pressDepth, 0f, t);
            mainRect.anchoredPosition = shadowOffset * pressAmount;
            borderRect.anchoredPosition = shadowOffset * pressAmount;
        
            var color = Color.Lerp(pressedColor, mainColor, t);
            mainImage.color = color;
            
            yield return null;
        }
    
        borderRect.anchoredPosition = Vector2.zero;
        mainRect.anchoredPosition = Vector2.zero;
        mainImage.color = mainColor;
    }
    
    private void ReloadLayers()
    {
        if (container)
            container.SetParent(_rect);
        
        if (shadowRect)
        {
            shadowRect.SetParent(null);
            Destroy(shadowRect.gameObject);
            shadowRect = null;
        }
        if (borderRect)
        {
            borderRect.SetParent(null);
            Destroy(borderRect.gameObject);
            borderRect = null;
        }
        if (mainRect)
        {
            mainRect.SetParent(null);
            Destroy(mainRect.gameObject);
            mainRect = null;
        }
        mainImage = null;

        LoadLayers(_rect);
    }

    private void LoadLayers(RectTransform rect)
    {
        shadowRect = CreateLayer("Shadow", rect, 0f, shadowOffset, shadowColor, 0, cornerShadow);
        borderRect = CreateLayer("Border", rect, 0f, Vector2.zero, borderColor, 1, cornerBorder);
        mainRect = CreateLayer("Main", rect, -borderThickness, Vector2.zero, mainColor, 2, cornerMain);
        mainImage = mainRect.GetComponent<Image>();

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
        
        return rect;
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
            
            if (container)
                container.SetParent(rect);
            
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
            mainImage = null;

            LoadLayers(rect);
            
            _delayedLoading = false;
        };
    }
    
#endif
}

}