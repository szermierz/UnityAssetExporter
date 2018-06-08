using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetsExporting
{ 

    public class PrefabInstancesGatheringRule : AssetRelationRuleBase
    {

        public PrefabInstancesGatheringRule() : base(typeof(GameObject))
        { }

        protected override List<Object> TryFindRelatedAssets(Object unityObject)
        {
            var result = base.TryFindRelatedAssets(unityObject);

            var gameObject = unityObject as GameObject;
            if (!gameObject)
                return result;

            var prefabType = PrefabUtility.GetPrefabType(gameObject);
            if (prefabType != PrefabType.PrefabInstance && prefabType != PrefabType.ModelPrefabInstance)
                return result;
             
            var asset = PrefabUtility.GetPrefabParent(gameObject);
            if (!asset)  
                return result;

            var assetPath = AssetDatabase.GetAssetPath(asset);
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            result.AddRange(assets);

            return result;
        }

    }

}