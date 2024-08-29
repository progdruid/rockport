using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(TransformOffsetPair))]
public class TransformOffsetPair_PropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();

        var transformField = new PropertyField(property.FindPropertyRelative("transform"), "Transform");
        var offsetField = new PropertyField(property.FindPropertyRelative("offset"), "Offset");

        container.Add(transformField);
        container.Add(offsetField);

        return container;
    }
}
