using System;
using System.IO;
using SimpleJSON;
using UnityEngine;

namespace MapEditor
{

public static class MapSaveManager
{

    public static bool SaveAs(string name, JSONObject data)
    {
        var directoryPath = Path.Combine(Application.dataPath, "Maps");
        var filePath = Path.Combine(directoryPath, name + ".json");

        try
        {
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(directoryPath);

            // Write the content to the file
            File.WriteAllText(filePath, data.ToString(4));
            Debug.Log($"File saved successfully at: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving file: {e.Message}");
            return false;
        }
    }

    public static bool Load(string name, out JSONObject data)
    {
        var filePath = Path.Combine(Application.dataPath, "Maps", name + ".json");
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            data = null;
            return false;
        }

        var text = File.ReadAllText(filePath);
        try
        {
            data = JSON.Parse(text).AsObject;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing file: {e.Message}");
            data = null;
            return false;
        }
        
        Debug.Log($"File loaded successfully from: {filePath}");
        return true;
    }

    public static bool Delete(string name)
    {
        var filePath = Path.Combine(Application.dataPath, "Maps", name + ".json");

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