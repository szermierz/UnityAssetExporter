using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetsExporting
{

    public class AssetDefinition
    {

        public string AssetPath { get; protected set; }

        protected List<AssetDefinition> m_RelatedAssets = new List<AssetDefinition>();
        public List<AssetDefinition> RelatedAssets { get { return m_RelatedAssets; } }

        public AssetDefinition(string assetPath)
        {
            AssetPath = assetPath;
        }

        public void AddRelatedAsset(AssetDefinition assetDefinition)
        {
            RelatedAssets.Add(assetDefinition);
        }

        public string Extension
        { get { return Path.GetExtension(AssetPath); } }

        public UnityEngine.Object Asset
        { get { return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetPath); } }

        public override string ToString()
        {
            return AssetPath;
        }

        public virtual void FromString(string value)
        {
            AssetPath = value;
        }

    }

}

