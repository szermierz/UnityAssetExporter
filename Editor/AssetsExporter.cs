using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetsExporting
{

    public sealed class AssetsExporter
    {

        public string DestFileDirectory { get; private set; }

        public AssetsExporter(string destFileDirectory)
        {
            DestFileDirectory = destFileDirectory;
        }

        private AssetDefinitionSerializer m_Serializer = null;

        public void ExportCurrentScene()
        {
            var sceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
            var destFilename = sceneName + ".txt";
            var objectsToExport = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().GetRootGameObjects();

            Export(destFilename, objectsToExport);
        }

        public void ExportAssetFromPath(string assetPath)
        {
            var filename = Path.GetFileName(assetPath);
            var destFilename = Path.GetFileNameWithoutExtension(filename) + ".txt";
            var objectsToExport = Enumerable.Repeat(AssetDatabase.LoadMainAssetAtPath(assetPath), 1);

            Export(destFilename, objectsToExport);
        }

        private void Export(string destFilename, IEnumerable<UnityEngine.Object> objectsToExport)
        {
            var destFilePath = DestFileDirectory + "/" + destFilename;

            m_Serializer = new AssetDefinitionSerializer(destFilePath);

            AssetRelationBuildingVisitor relationBuilder = new AssetRelationBuildingVisitor();

            relationBuilder.OnObjectEntered -= OnAssetEntered;
            relationBuilder.OnObjectEntered += OnAssetEntered;

            relationBuilder.Build(objectsToExport);

            m_Serializer.Serialize();
            m_Serializer = null;
        }

        private void OnAssetEntered(UnityEngine.Object unityObject)
        {
            if(null == m_Serializer)
                return;

            var assetsFolderPath = Application.dataPath.TrimEnd('/', '\\');
            if(assetsFolderPath.EndsWith("Assets"))
                assetsFolderPath = assetsFolderPath.Substring(0, assetsFolderPath.Length - ("Assets").Length).TrimEnd('/', '\\');

            var assetPath = assetsFolderPath + "/" + AssetDatabase.GetAssetPath(unityObject);
            var assetDefinition = new AssetDefinition(assetPath);

            m_Serializer.AddAssetDefinition(assetDefinition);
        }

    }

}