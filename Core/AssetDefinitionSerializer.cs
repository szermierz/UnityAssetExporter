using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetsExporting
{

    public class AssetDefinitionSerializationBase
    {

        public readonly string FilePath;

        public AssetDefinitionSerializationBase(string filePath)
        {
            FilePath = filePath;
        }

        protected List<AssetDefinition> m_AssetDefinitions = new List<AssetDefinition>();

        protected readonly string c_AssetSeparator = "\n";

    }

    public class AssetDefinitionSerializer : AssetDefinitionSerializationBase
    {

        public AssetDefinitionSerializer(string filePath) : base(filePath)
        { }

        public void AddAssetDefinition(AssetDefinition assetDefinition)
        {
            m_AssetDefinitions.Add(assetDefinition);
        }

        public void Serialize()
        {
            foreach(var assetDefinition in m_AssetDefinitions)
                File.AppendText(assetDefinition.ToString() + c_AssetSeparator);
        }

    }

    public class AssetDefinitionDeserializer : AssetDefinitionSerializationBase
    {

        public AssetDefinitionDeserializer(string filePath) : base(filePath)
        { }

        public IEnumerator<AssetDefinition> Deserialize()
        {
            var fileContent = File.ReadAllText(FilePath);
            var assetPaths = fileContent.Split(new string[] { c_AssetSeparator }, System.StringSplitOptions.RemoveEmptyEntries);

            m_AssetDefinitions.Clear();

            foreach(var assetPath in assetPaths)
                m_AssetDefinitions.Add(new AssetDefinition(assetPath));

            return m_AssetDefinitions.GetEnumerator();
        }

    }

}