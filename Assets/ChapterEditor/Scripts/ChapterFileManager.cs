using System;
using System.IO;
using UnityEngine;

namespace ChapterEditor
{

public static class ChapterFileManager
{

    public static bool SaveAs(string name, string data)
    {
        var directoryPath = Path.Combine(Application.persistentDataPath, "Levels");
        var filePath = Path.Combine(directoryPath, name + ".json");

        try
        {
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(directoryPath);

            // Write the content to the file
            File.WriteAllText(filePath, data);
            Debug.Log($"File saved successfully at: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving file: {e.Message}");
            return false;
        }
    }

    public static bool Load(string name, out string data)
    {
        var filePath = Path.Combine(Application.persistentDataPath, "Levels", name + ".json");
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            data = null;
            return false;
        }

        data = File.ReadAllText(filePath);
        Debug.Log($"File loaded successfully from: {filePath}");
        return true;
    }

    public static bool Delete(string name)
    {
        var filePath = Path.Combine(Application.persistentDataPath, "Levels", name + ".json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return false;
        }

        File.Delete(filePath);
        Debug.Log($"File deleted successfully: {filePath}");
        return true;
    }
}

}