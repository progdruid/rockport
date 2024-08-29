using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Reflection;

public class DynamicTypeField : VisualElement
{
    private object value;
    private Type fieldType;

    public DynamicTypeField(Type type, object initialValue, bool isReadOnly, Action<object> onValueChanged)
    {
        fieldType = type;
        value = initialValue;

        if (fieldType == typeof(int))
        {
            var field = new IntegerField();
            field.value = (int)value;
            field.RegisterValueChangedCallback(evt => 
            {
                value = evt.newValue;
                onValueChanged(value);
            });
            field.isReadOnly = isReadOnly;
            Add(field);
        }
        else if (fieldType == typeof(float))
        {
            var field = new FloatField();
            field.value = (float)value;
            field.RegisterValueChangedCallback(evt => 
            {
                value = evt.newValue;
                onValueChanged(value);
            });
            field.isReadOnly = isReadOnly;
            Add(field);
        }
        else if (fieldType == typeof(string))
        {
            var field = new TextField();
            field.value = (string)value;
            field.RegisterValueChangedCallback(evt => 
            {
                value = evt.newValue;
                onValueChanged(value);
            });
            field.isReadOnly = isReadOnly;
            Add(field);
        }
        else if (fieldType == typeof(bool))
        {
            var field = new Toggle();
            field.value = (bool)value;
            field.RegisterValueChangedCallback(evt => 
            {
                value = evt.newValue;
                onValueChanged(value);
            });
            field.SetEnabled(!isReadOnly);
            Add(field);
            
        }
        else if (fieldType == typeof(Vector2))
        {
            var field = new Vector2Field();
            field.value = (Vector2)value;
            field.RegisterValueChangedCallback(evt => 
            {
                value = evt.newValue;
                onValueChanged(value);
            });
            field.SetEnabled(!isReadOnly);
            Add(field);
        }
        else if (fieldType == typeof(Vector3))
        {
            var field = new Vector3Field();
            field.value = (Vector3)value;
            field.RegisterValueChangedCallback(evt => 
            {
                value = evt.newValue;
                onValueChanged(value);
            });
            field.SetEnabled(!isReadOnly);
            Add(field);
        }
        else if (fieldType == typeof(Color))
        {
            var field = new ColorField();
            field.value = (Color)value;
            field.RegisterValueChangedCallback(evt => 
            {
                value = evt.newValue;
                onValueChanged(value);
            });
            field.SetEnabled(!isReadOnly);
            Add(field);
        }
        else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
        {
            var field = new ObjectField();
            field.objectType = fieldType;
            field.value = (UnityEngine.Object)value;
            field.RegisterValueChangedCallback(evt => 
            {
                value = evt.newValue;
                onValueChanged(value);
            });
            field.SetEnabled(!isReadOnly);
            Add(field);
        }
        else if (fieldType.IsEnum)
        {
            var field = new EnumField();
            field.Init((Enum)value);
            field.RegisterValueChangedCallback(evt => 
            {
                value = evt.newValue;
                onValueChanged(value);
            });
            field.SetEnabled(!isReadOnly);
            Add(field);
        }
        else
        {
            Add(new Label($"Unsupported type: {fieldType.Name}"));
        }
    }
    
    public object GetValue() => value;
    public Type GetFieldType() => fieldType;
}