using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class SaveLoadUIPanel : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private TMP_InputField pathInputField;
    [SerializeField] private ButtonWithEvents saveButton;
    [SerializeField] private ButtonWithEvents loadButton;
    [SerializeField] private ButtonWithEvents deleteButton;
    [SerializeField] private EditorController editorController;

    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(pathInputField);
        Assert.IsNotNull(saveButton);
        Assert.IsNotNull(loadButton);
        Assert.IsNotNull(deleteButton);

        saveButton.onClick.AddListener(HandleSaveButtonClick);
        loadButton.onClick.AddListener(HandleLoadButtonClick);
        deleteButton.onClick.AddListener(HandleDeleteButtonClick);

        pathInputField.onSelect.AddListener(s => HandleInteractChange(true));
        pathInputField.onDeselect.AddListener(s => HandleInteractChange(false));
        saveButton.InteractChangeEvent += HandleInteractChange;
        loadButton.InteractChangeEvent += HandleInteractChange;
    }

    private void OnDisable()
    {
        saveButton.onClick.RemoveListener(HandleSaveButtonClick);
        loadButton.onClick.RemoveListener(HandleLoadButtonClick);
        deleteButton.onClick.RemoveListener(HandleDeleteButtonClick);

        pathInputField.onSelect.RemoveListener(s => HandleInteractChange(true));
        pathInputField.onDeselect.RemoveListener(s => HandleInteractChange(false));
        saveButton.InteractChangeEvent -= HandleInteractChange;
        loadButton.InteractChangeEvent -= HandleInteractChange;
    }

    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void HandleInteractChange(bool interacting)
    {
        EditorController.CanEdit = !interacting;
    }

    private void HandleSaveButtonClick()
    {
        var fileName = pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        var content = editorController.Serialize();
        var directoryPath = Path.Combine(Application.persistentDataPath, "Levels");
        var filePath = Path.Combine(directoryPath, fileName + ".json");

        try
        {
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(directoryPath);

            // Write the content to the file
            File.WriteAllText(filePath, content);
            Debug.Log($"File saved successfully at: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving file: {e.Message}");
        }
    }

    private void HandleLoadButtonClick()
    {
        var fileName = pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        var filePath = Path.Combine(Application.persistentDataPath, "Levels", fileName + ".json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        var content = File.ReadAllText(filePath);
        editorController.Deserialize(content);
        Debug.Log($"File loaded successfully from: {filePath}");
    }

    private void HandleDeleteButtonClick()
    {
        var fileName = pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        var filePath = Path.Combine(Application.persistentDataPath, "Levels", fileName + ".json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        File.Delete(filePath);
        Debug.Log($"File deleted successfully: {filePath}");
    }
}