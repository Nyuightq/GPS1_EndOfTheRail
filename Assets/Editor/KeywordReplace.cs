// Assets/Editor/KeywordReplace.cs

using UnityEngine;
using UnityEditor;
using System.IO;

public class KeywordReplace : UnityEditor.AssetModificationProcessor
{
    public static void OnWillCreateAsset(string path)
    {
        path = path.Replace(".meta", "");
        int index = path.LastIndexOf(".");
        if (index < 0) return;

        string file = path.Substring(index);
        if (file != ".cs") return; // Only process C# scripts

        index = Application.dataPath.LastIndexOf("Assets");
        path = Application.dataPath.Substring(0, index) + path;

        if (!File.Exists(path)) return;

        string fileContent = File.ReadAllText(path);

        // Replace tokens
        fileContent = fileContent.Replace("#CREATIONDATE#", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        fileContent = fileContent.Replace("#PROJECTNAME#", PlayerSettings.productName);
        fileContent = fileContent.Replace("#DEVELOPER#", System.Environment.UserName);

        File.WriteAllText(path, fileContent);
        AssetDatabase.Refresh();
    }
}
