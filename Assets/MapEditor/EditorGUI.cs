using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace MapEditor
{

public class EditorGUI : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private TMP_InputField pathInputField;
    [SerializeField] private ButtonWithEvents saveButton;
    [SerializeField] private ButtonWithEvents loadButton;
    [SerializeField] private ButtonWithEvents deleteButton;
    [SerializeField] private TMP_Dropdown entityCreationDropdown;
    
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

        var content = editorController.Pack();
        MapSaveManager.SaveAs(fileName, content);
    }

    private void HandleLoadButtonClick()
    {
        var fileName = pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        if (MapSaveManager.Load(fileName, out var content))
            editorController.Unpack(content);
    }

    private void HandleDeleteButtonClick()
    {
        var fileName = pathInputField.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty. Please enter a valid file name.");
            return;
        }

        MapSaveManager.Delete(fileName);
    }
}

}