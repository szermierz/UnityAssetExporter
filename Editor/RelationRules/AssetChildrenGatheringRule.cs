using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetsExporting
{ 

    public class AssetChildrenGatheringRule : AssetRelationRuleBase
    {

        public AssetChildrenGatheringRule() : base(typeof(UnityEngine.Object))
        { }

        protected override List<Object> TryFindRelatedAssets(Object unityObject)
        {
            var result = base.TryFindRelatedAssets(unityObject);

            var assetPath = AssetDatabase.GetAssetPath(unityObject);
            var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            result.AddRange(assetsAtPath);

            return result;
        }

    }

}