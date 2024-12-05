using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace ChapterEditor
{

public class TextPropertyUIField : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private TMP_Text textField;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private RectTransform target;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(textField);
        Assert.IsNotNull(inputField);
        Assert.IsNotNull(target);
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public RectTransform Target => target;

    public void SetProperty(PropertyHandle handle)
    {
        textField.text = handle.PropertyName + ": ";
        textField.rectTransform.sizeDelta = new Vector2(textField.preferredWidth, textField.rectTransform.sizeDelta.y);
        if (inputField.transform is RectTransform inputRect)
            inputRect.anchoredPosition =
                new Vector2(textField.preferredWidth, textField.rectTransform.anchoredPosition.y);

        inputField.text = handle.Getter.Invoke().ToString();
        inputField.contentType = handle.PropertyType switch
        {
            PropertyType.Decimal => TMP_InputField.ContentType.DecimalNumber,
            PropertyType.Integer => TMP_InputField.ContentType.IntegerNumber,
            PropertyType.Text => TMP_InputField.ContentType.Standard,
            _ => throw new ArgumentOutOfRangeException()
        };
        inputField.onSelect.AddListener((s) => EditorController.CanEdit = false);
        inputField.onEndEdit.AddListener((s) =>
        {
            object val = handle.PropertyType switch
            {
                PropertyType.Decimal => float.Parse(s),
                PropertyType.Integer => int.Parse(s),
                PropertyType.Text => s,
                _ => throw new ArgumentOutOfRangeException()
            };
            handle.Setter.Invoke(val);
            EditorController.CanEdit = true;
            inputField.text = handle.Getter.Invoke().ToString();
        });
    }
}

}