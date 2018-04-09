using System;
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
            try
            {
                File.WriteAllText(FilePath, "");

                foreach(var assetDefinition in m_AssetDefinitions)
                    File.AppendAllText(FilePath, assetDefinition.ToString() + c_AssetSeparator);
            }
            catch(Exception)
            { }
        }

    }

    public class AssetDefinitionDeserializer : AssetDefinitionSerializationBase
    {

        public AssetDefinitionDeserializer(string filePath) : base(filePath)
        { }

        public IEnumerable<AssetDefinition> Deserialize()
        {
            string fileContent = "";
            try
            { fileContent = File.ReadAllText(FilePath); }
            catch(Exception)
            { }

            var assetPaths = fileContent.Split(new string[] { c_AssetSeparator }, System.StringSplitOptions.RemoveEmptyEntries);

            m_AssetDefinitions.Clear();

            foreach(var assetPath in assetPaths)
                m_AssetDefinitions.Add(new AssetDefinition(assetPath));

            return m_AssetDefinitions;
        }

    }

}