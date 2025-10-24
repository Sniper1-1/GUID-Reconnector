using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ImportGuidReferences : GUID_Reconnector_Core
{
    private string jsonPath = null; //path to import json file

    //this is where the button for the tool is up in the toolbar and the window it creates
    [MenuItem("Tools/GUID Reconnector/Import Info")]
    public static void OpenImportWindow()
    {
        GetWindow<ImportGuidReferences>("Import GUIDs");
    }

    //what the window looks like
    private void OnGUI()
    {
        GUILayout.Label("Import GUID Reference Info", EditorStyles.boldLabel);

        movingPath = PathField("Folder to move", movingPath);
        basePath = PathField("Base folder", basePath);


        GUILayout.Space(10);
        if (GUILayout.Button("Import and Reconnect Connections\nUsing Saved JSON"))
        {
            jsonPath = EditorUtility.OpenFilePanel("Select JSON File", "Assets", "json"); //open file panel set to only show json files
            if (!string.IsNullOrEmpty(jsonPath))//if json selected, do the import
            {
                ImportInfo(movingPath, basePath, jsonPath);
            }
            else
            {
                Debug.LogError("No JSON file selected.");
            }

        }
    }

    //the main import code
    private void ImportInfo(string movingPath, string basePath, string jsonPath)
    {
        var window = GetWindow<ImportGuidReferences>();
        window.ClearLog();//clear the log first so that previous runs don't clutter it
        window.WriteLog("GUID Import\n\n");

        if (!Directory.Exists(movingPath) || !Directory.Exists(basePath))
        {
            window.WriteLog("One or more folders do not exist. Please check your paths.", MSGType.ERROR);
            return;
        }

        if (!File.Exists(jsonPath))
        {
            window.WriteLog("JSON file not found: " + jsonPath, MSGType.ERROR);
            return;
        }

        window.WriteLog($"Loaded JSON from {jsonPath}");
        string json = File.ReadAllText(jsonPath); //read everything from the json
        GuidReferenceExport importData = JsonUtility.FromJson<GuidReferenceExport>(json);//and parse it into the data structure

        if (importData == null || importData.entries == null)
        {
            window.WriteLog("Failed to parse JSON or no entries found.", MSGType.ERROR);
            return;
        }

        window.WriteLog($"Successfully loaded {importData.entries.Count} entries from JSON.");

        //build lookup of new GUIDs
        Dictionary<string, string> currentNameToGuid = new();
        string[] baseMetaFiles = Directory.GetFiles(basePath, "*.meta", SearchOption.AllDirectories); //get all meta files in the base path
        
        for (int i = 0; i < baseMetaFiles.Length; i++) //loop through all meta files
        {
            string metaPath = baseMetaFiles[i]; //get the current meta file path
            float progress = (float)i / baseMetaFiles.Length; //used for the progress bar
            window.ShowProgressBar("Building GUID Lookup", $"Processing base meta files... {i+1} / {baseMetaFiles.Length}", progress);
            
            string guid = GetGuidFromMeta(metaPath);//get the GUID from the current meta file
            if (guid != null)
            {
                string assetName = Path.GetFileNameWithoutExtension(metaPath);
                currentNameToGuid[assetName] = guid;
            }
        }
        window.ClearProgressBar(); //clear the progress bar

        //replace old GUIDs with the new ones
        int replacementCount = 0;
        int totalEntries = importData.entries.Count;
        
        for(int i= 0; i < totalEntries; i++)
        {
            var entry = importData.entries[i];
            float progress = (float)i / totalEntries; //used for the progress bar
            window.ShowProgressBar("Reconnecting GUIDs", $"Reconnecting entries... {i+1} / {totalEntries}", progress);
            
            if (!currentNameToGuid.TryGetValue(entry.assetName, out string newGuid))
            {
                window.WriteLog($"No matching asset found for '{entry.assetName}' — skipping.", MSGType.WARNING);
                continue;
            }

            //go through all files that referenced the old GUID and replace it with the new one
            foreach (string modFile in entry.modFiles)
            {
                if (!File.Exists(modFile)) //skip a file that somehow doesn't exist anymore
                {
                    continue;
                }
                string text = File.ReadAllText(modFile); //read the file being modified
                if (text.Contains(entry.guid)) //and if it contains the old GUID
                {
                    text = text.Replace(entry.guid, newGuid); //swap it for the new
                    File.WriteAllText(modFile, text); //and write it back to disk
                    replacementCount++;
                    window.WriteLog($"Replaced {entry.guid} → {newGuid} in {modFile}");
                }
            }
        }
        window.ClearProgressBar(); //clear the progress bar

        AssetDatabase.Refresh(); //refresh the asset database to reflect any changes
        window.WriteLog($"Finished reconnecting. {replacementCount} files updated.");
        window.SaveLog(jsonPath, "import");
    }
}