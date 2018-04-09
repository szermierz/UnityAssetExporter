using System;
using System.Collections.Generic;
using System.IO;
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

        public void Export(string assetPath)
        {
            var filename = Path.GetFileName(assetPath);
            var destFilePath = DestFileDirectory + "/" + Path.GetFileNameWithoutExtension(filename) + ".txt";

            m_Serializer = new AssetDefinitionSerializer(destFilePath);
            AssetRelationBuildingVisitor relationBuilder = new AssetRelationBuildingVisitor();

            relationBuilder.OnObjectEntered -= OnAssetEntered;
            relationBuilder.OnObjectEntered += OnAssetEntered;

            var prefabToExport = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            relationBuilder.Build(prefabToExport);

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