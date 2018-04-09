using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AssetsExporting;
using System.IO;

public class AssetsImportingWindow : EditorWindow
{

    [MenuItem("Window/Assets importing tools")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AssetsImportingWindow));
    }

    void OnGUI()
    {
        if(GUILayout.Button("export-test"))
        {
            var selected = Selection.assetGUIDs;
            if(selected.Length == 0)
                return;

            var path = AssetDatabase.GUIDToAssetPath(selected[0]);

            var destPath = EditorUtility.SaveFolderPanel("dasdas", "", "dasda");

            var exporter = new AssetsExporter(destPath);

            exporter.Export(path);
        }

        if(GUILayout.Button("import-test"))
        {
            var destPath = EditorUtility.SaveFolderPanel("dasdas", "", "dasda");
            
            var descriptorsFolder = EditorUtility.OpenFolderPanel("dasdas", "", "dasda");
            var files = Directory.EnumerateFiles(descriptorsFolder);

            foreach(var file in files)
            {
                var folder = destPath + "/" + Path.GetFileNameWithoutExtension(file);
                var importer = new AssetsImporter(folder);
                importer.Import(file);
            }
        }
    }
}
