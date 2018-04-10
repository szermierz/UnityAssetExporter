using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AssetsExporting;
using System.IO;
using System;

public class AssetsImportingWindow : EditorWindow
{

    [MenuItem("Window/Assets importing tools")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(AssetsImportingWindow));
        window.minSize = new Vector2(670, 270);
        window.maxSize = new Vector2(671, 271);
    }

    void OnGUI()
    {
        GUILayout.Label("Following tool allows you to export and import assets from one Unity repo to another");
        GUILayout.Label("To do so, complete following steps:");
        GUILayout.Label("1. Open UnityEditor of repository you want to export assets from");
        GUILayout.Label("2. Open this dialog");
        GUILayout.Label("3. In Project Tab: Select one or many (to select many you may use ctrl + left click) prefabs you want to export");
        GUILayout.Label("4. Use \"Export assets\" button of this dialog");
        GUILayout.Label("5. Close UnityEditor of this repo and open UnityEditor of repo you want import assets to");
        GUILayout.Label("6. Use \"Import assets\" button of this dialog");
        GUILayout.Space(50);

        if(GUILayout.Button("Export assets selected in Project Tab into export info files"))
        {
            var selected = Selection.assetGUIDs;
            if(selected.Length == 0)
            {
                Debug.LogError("[Assets Importing Window] Couldn't export empty selection!");
                return;
            }

            var exportDestPath = EditorUtility.SaveFolderPanel("Select destination folder to save export info",
                                                               System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                                                               "AssetsExportInfo");
            var exporter = new AssetsExporter(exportDestPath);

            foreach(var selectedObject in selected)
            {
                var path = AssetDatabase.GUIDToAssetPath(selectedObject);
                exporter.Export(path);
            }
        }

        if(GUILayout.Button("Import assets from a folder containing export info files"))
        {
            var destPath = EditorUtility.SaveFolderPanel("Select destination folder to import assets to", "", "");
            
            var descriptorsFolder = EditorUtility.OpenFolderPanel("Select folder containing exported assets info files", 
                                                                  System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                                                                  "AssetsExportInfo");

            IEnumerable<string> files = null;
            try
            { files = Directory.GetFiles(descriptorsFolder); }
            catch(Exception)
            {
                Debug.LogError("[Assets Importing Window] Couldn't find any export info files!");
                return;
            }

            foreach(var file in files)
            {
                var folder = destPath + "/" + Path.GetFileNameWithoutExtension(file);
                var importer = new AssetsImporter(folder);
                importer.Import(file);
            }
        }
    }
}
