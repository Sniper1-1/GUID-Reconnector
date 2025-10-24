using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExtractGuidReferences : GUID_Reconnector_Core
{
    //this is where the button for the tool is up in the toolbar and the window it creates
    [MenuItem("Tools/GUID Reconnector/Extract Info")]
    public static void ShowExtractWindow()
    {
        GetWindow<ExtractGuidReferences>("Extract GUIDs");
    }

    //what the window looks like
    void OnGUI()
    {
        GUILayout.Label("Extract GUID Reference Info", EditorStyles.boldLabel);

        movingPath = PathField("Folder to move", movingPath);
        basePath = PathField("Base folder", basePath);

        GUILayout.Space(10);
        if (GUILayout.Button("Extract and Save JSON")) //if the button is clicked, do the function
        {
            ExtractAndSave(movingPath, basePath);
        }
    }

    //the main extraction code
    private static void ExtractAndSave(string movePath, string basePath)
    {
        var window = GetWindow<ExtractGuidReferences>();
        window.ClearLog();//clear the log first so that previous runs don't clutter it
        window.WriteLog("GUID Extraction\n\n");

        if (!Directory.Exists(movePath) || !Directory.Exists(basePath))
        {
            window.WriteLog("One or more folders do not exist. Please check your paths.", MSGType.ERROR);
            return;
        }

        // Ask user where to save JSON file
        string defaultName = "GuidReferenceMap.json";
        string savePath = EditorUtility.SaveFilePanel(
            "Save GUID Reference JSON",
            Application.dataPath,
            defaultName,
            "json"
        );

        if (string.IsNullOrEmpty(savePath))
        {
            window.WriteLog("Export canceled, no save location.", MSGType.ERROR);
            return;
        }

        // Map duplicated GUIDs to filenames
        Dictionary<string, string> duplicatedGuidToName = new();
        string[] BaseMetaFiles = Directory.GetFiles(basePath, "*.meta", SearchOption.AllDirectories);
        
        for (int i = 0; i < BaseMetaFiles.Length; i++)//go through the base meta files
        {
            string metaPath = BaseMetaFiles[i];//get the current meta file path
            float progress = (float)i / BaseMetaFiles.Length;//used to calculate progress
            window.ShowProgressBar("Extracting GUIDs", $"Scanning base folder({i + 1}/{BaseMetaFiles.Length})", progress);//update progress bar

            string guid = GetGuidFromMeta(metaPath); //and get the GUID from the current meta file
            if (guid != null) //if it exists
            {
                string assetName = Path.GetFileNameWithoutExtension(metaPath); //get the asset name
                duplicatedGuidToName[guid] = assetName; //and store it in the dictionary
                window.WriteLog($"Found GUID: {guid} for asset: {assetName} in: {metaPath}");
            }
            else
            {
                window.WriteLog($"No GUID found in meta file: {metaPath}", MSGType.WARNING);
            }
        }
        window.ClearProgressBar();//clear the progress bar when done
        // Find where those GUIDs are referenced in what's being moved
        List<GuidReferenceEntry> entries = new();
        string[] MoveFiles= Directory.GetFiles(movePath, "*.*", SearchOption.AllDirectories);
        
        for(int i=0;i< MoveFiles.Length;i++)//go through everything that's being moved
        {
            string filePath = MoveFiles[i];//get the current file path
            float progress = (float)i / MoveFiles.Length;//used to calculate progress
            window.ShowProgressBar("Extracting GUIDs", $"Scanning moving folder({i + 1}/{MoveFiles.Length})", progress);//update progress bar
                
            if (filePath.EndsWith(".meta") || filePath.EndsWith(".cs")) //skip
            {
                continue;
            }

            string text = File.ReadAllText(filePath); //read the current file being moved

            foreach (var kvp in duplicatedGuidToName) //go through the GUID key-value-pairs
            {
                if (text.Contains(kvp.Key)) //if the file contains a reference to the GUID
                {
                    GuidReferenceEntry entry = entries.Find(e => e.guid == kvp.Key); //see if we already have an entry for it
                    if (entry == null) //if not, create a new one
                    {
                        entry = new GuidReferenceEntry
                        {
                            guid = kvp.Key,
                            assetName = kvp.Value,
                            modFiles = new List<string>()
                        };
                        entries.Add(entry);
                        window.WriteLog($"Found reference to GUID: {kvp.Key} on asset: {kvp.Value} in file: {filePath}");
                    }
                    else
                    {
                        window.WriteLog($"Found additional reference to GUID: {kvp.Key} on asset: {kvp.Value} in file: {filePath}");
                    }
                    entry.modFiles.Add(filePath); //add the file to the list of modified files for this GUID
                }
            }
        }
        window.ClearProgressBar();//clear the progress bar when done

        GuidReferenceExport export = new() { entries = entries }; //create the export data structure

        //write the JSON file
        string json = JsonUtility.ToJson(export, true);
        File.WriteAllText(savePath, json, Encoding.UTF8);

        //write the log file
        window.WriteLog($"Extracted GUID reference data.\nFound {entries.Count} referenced duplicated assets.\nSaved to: {savePath}");
        window.SaveLog(savePath, "extraction");

        AssetDatabase.Refresh(); //refresh the asset database to reflect any changes
    }
}
