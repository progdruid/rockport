using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.UIElements;
using Object = System.Object;

[CustomPropertyDrawer(typeof(SerializableMap<,>))]
public class DictionaryDrawer : PropertyDrawer
{
    private const float ButtonWidth = 20f;

    private object keyToAdd = null;
    private Type keyType;
    private Type valueType;

    private UnityEngine.Object targetObject;
    
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        targetObject = property.serializedObject.targetObject;

        var dict = GetDictionary(property);
        var genericArgs = dict.GetType().GetGenericArguments();
        keyType = genericArgs[0];
        keyToAdd = CreateInstance(keyType);
        valueType = genericArgs[1];

        
        var container = new VisualElement();
        
        var foldout = new Foldout () { text = property.displayName };
        container.Add(foldout);
        
        var listView = new ListView();
        listView.makeItem = () => new MapItemView();
        listView.bindItem = (element, index) =>
        {
            var mapItemView = (MapItemView)element;
            object key = null;
            int i = 0;
            foreach (DictionaryEntry current in dict)
            {
                if (i == index)
                {
                    key = current.Key;
                    break;
                }
                i++;
            }

            if (key != null)
                mapItemView.SetItem(key, dict,
                    () => RemoveItem(dict, key, property, listView), 
                    RefreshAsset);
        };
        RefreshListView(listView, dict);
        
        
        var keyToAddField = new DynamicTypeField(keyType, keyToAdd, false,
            (key) =>
            {
                keyToAdd = key;
            });
        
        var addButton = new UnityEngine.UIElements.Button(
            () =>
            {
                AddNewItem(dict, keyToAdd, property, listView);
                keyToAdd = CreateInstance(keyType);
            });
        addButton.text = "+";
        
        foldout.Add(listView);

        var addNewItemContainer = new VisualElement();
        addNewItemContainer.style.flexDirection = FlexDirection.Row;
        addNewItemContainer.style.alignContent = Align.Stretch;
        addNewItemContainer.style.justifyContent = Justify.FlexEnd;
        
        addNewItemContainer.Add(new Label("A new item will be added with this key:"));
        addNewItemContainer.Add(keyToAddField);
        addNewItemContainer.Add(addButton);
        
        
        foldout.Add(addNewItemContainer);
        
        return container;
    }
    
    private void RefreshListView(ListView listView, IDictionary dict)
    {
        List<DictionaryEntry> entries = new();
        foreach (DictionaryEntry entry in dict)
            entries.Add(entry);
        listView.itemsSource = entries;
        listView.RefreshItems();
    }

    private void RefreshAsset()
    {
        EditorUtility.SetDirty(targetObject);
        AssetDatabase.SaveAssets();
    }

    private IDictionary GetDictionary(SerializedProperty property)
    {
        object target = property.serializedObject.targetObject;
        var field = target.GetType().GetField(property.propertyPath);
        return (IDictionary)field.GetValue(target);
    }

    private void AddNewItem(IDictionary dict, object key, SerializedProperty property, ListView listView)
    {
        var genericArgs = dict.GetType().GetGenericArguments();
        var value = CreateInstance(valueType);
        
        if (key != null && !dict.Contains(key))
        {
            dict.Add(key, value);
            property.serializedObject.ApplyModifiedProperties();
            RefreshListView(listView, dict);
            RefreshAsset();
        }
        else
        {
            Debug.LogWarning("Failed to add new item to dictionary. Key is null or already exists.");
        }
    }

    private void RemoveItem(IDictionary dict, object key, SerializedProperty property, ListView listView)
    {
        dict.Remove(key);
        property.serializedObject.ApplyModifiedProperties();
        RefreshListView(listView, dict);
        RefreshAsset();
    }
    
    private object CreateInstance(Type t)
    {
        if (t == typeof(string))
            return "";

        return t.IsValueType ? Activator.CreateInstance(t) : null;
    }
}

public class MapItemView : VisualElement
{
    private DynamicTypeField keyField;
    private DynamicTypeField valueField;
    private UnityEngine.UIElements.Button removeButton;
    
    public MapItemView()
    {
        removeButton = new UnityEngine.UIElements.Button() { text = "-" };
        
        Add(removeButton);

        style.flexDirection = FlexDirection.Row;
        style.alignContent = Align.Stretch;
        //style.justifyContent = Justify.;
    }

    public void SetItem(object key, IDictionary dict, Action removeAction, Action itemChangedAction)
    {
        if (keyField != null)
            Remove(keyField);
        if (valueField != null)
            Remove(valueField);
        
        var genericArgs = dict.GetType().GetGenericArguments();
        var keyType = genericArgs[0];
        var valueType = genericArgs[1];
        
        keyField = new DynamicTypeField(keyType, key, true,
            (newKey) => { });
        valueField = new DynamicTypeField(valueType, dict[key], false,
            (newValue) =>
            {
                dict[key] = newValue;
                itemChangedAction();
            });
        
        Add(keyField);
        Add(valueField);
        
        removeButton.clicked += removeAction;
    }
}