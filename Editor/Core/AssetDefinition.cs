using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetsExporting
{

    public class AssetDefinition
    {

        public string AssetPath { get; protected set; }

        public AssetDefinition(string assetPath)
        {
            AssetPath = assetPath;
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

