using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SerializableMap<,>))]
public class MapDrawer : PropertyDrawer
{
    private object keyToAdd = null;
    private Type keyType;
    private Type valueType;

    private UnityEngine.Object targetObject;
    
    private StyleSheet styleSheet;
    
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        targetObject = property.serializedObject.targetObject;

        var dict = GetDictionary(property);
        var genericArgs = dict.GetType().GetGenericArguments();
        keyType = genericArgs[0];
        keyToAdd = CreateInstance(keyType);
        valueType = genericArgs[1];

        styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/MapDrawer.uss");

        var foldout = new Foldout () { text = property.displayName };
        foldout.styleSheets.Add(styleSheet);
        foldout.AddToClassList("map-drawer__foldout");

        var listContainer = new VisualElement();
        listContainer.AddToClassList("map-drawer__list-container");
        foldout.Add(listContainer);
        
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
        
        listContainer.Add(listView);

        var addNewItemContainer = new VisualElement();
        addNewItemContainer.AddToClassList("map-drawer__add-item-container");

        var addItemLabel = new Label("Add new item with key:");
        addItemLabel.AddToClassList("map-drawer__add-item-label");
        keyToAddField.AddToClassList("map-drawer__add-item-key-field");
        addButton.AddToClassList("map-drawer__add-button");
        addNewItemContainer.Add(addItemLabel);
        addNewItemContainer.Add(keyToAddField);
        addNewItemContainer.Add(addButton);
        
        
        listContainer.Add(addNewItemContainer);
        
        return foldout;
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
        var field = target.GetType().GetField(property.propertyPath, 
            BindingFlags.Public | 
            BindingFlags.NonPublic | 
            BindingFlags.Instance);
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
        removeButton.AddToClassList("map-item-view__remove-button");
        
        Add(removeButton);

        AddToClassList("map-item-view");
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
        
        keyField.AddToClassList("map-item-view__key-field");
        valueField.AddToClassList("map-item-view__value-field");
        
        Add(keyField);
        Add(valueField);
        
        removeButton.clicked += removeAction;
    }
}