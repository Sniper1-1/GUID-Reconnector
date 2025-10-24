using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class GUID_Reconnector_Core : EditorWindow
{
    //directory paths to reconnect
    protected string movingPath = null;
    protected string basePath = null;

    //UI helper for folder selection
    protected string PathField(string label, string path)
    {
        EditorGUILayout.BeginHorizontal();
        path = EditorGUILayout.TextField(label, path);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(60)))
        {
            string selected = EditorUtility.OpenFolderPanel($"Select {label}", "Assets", "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                    selected = "Assets" + selected.Substring(Application.dataPath.Length);
                path = selected;
            }
        }
        EditorGUILayout.EndHorizontal();
        return path;
    }

    //Helper to read GUID from .meta file
    protected static string GetGuidFromMeta(string metaPath)
    {
        foreach (string line in File.ReadLines(metaPath))
        {
            if (line.StartsWith("guid: "))
                return line.Substring(6).Trim();
        }
        return null;
    }

    //Data structures for JSON export/import
    [System.Serializable]
    protected class GuidReferenceEntry
    {
        public string guid;
        public string assetName;
        public List<string> modFiles;
    }

    [System.Serializable]
    protected class GuidReferenceExport
    {
        public List<GuidReferenceEntry> entries;
    }

    //Logging functions
    private StringBuilder logText = new();
    protected enum MSGType { INFO, WARNING, ERROR }
    protected void WriteLog(string message, MSGType type = MSGType.INFO)
    {
        if (type == MSGType.WARNING) //append some labels before the message if needed to make it easier to find when reading the log
        {
            message = "WARNING: " + message;
            Debug.LogWarning(message);
        }
        else if (type == MSGType.ERROR)
        {
            message = "ERROR: " + message;
            Debug.LogError(message);
        }
        else
        {
            Debug.Log(message);
        }
        logText.AppendLine(message);

    }
    protected void SaveLog(string targetPath, string suffix)
    {
        if (!string.IsNullOrEmpty(targetPath))
        {
            string logPath = Path.ChangeExtension(targetPath, $"{suffix}.log");
            File.WriteAllText(logPath, logText.ToString(), Encoding.UTF8);
            Debug.Log($"Log saved to: {logPath}");
        }
        else
        {
            Debug.LogWarning("No target path provided for log file.");
        }
    }
    protected void ClearLog()
    {
        logText.Clear();
    }

    //Functions used to show loading bars
    protected void ShowProgressBar(string title, string info, float progress)
    {
        EditorUtility.DisplayProgressBar(title, info, progress);
    }
    protected void ClearProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }
}