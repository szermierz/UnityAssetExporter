using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AssetsExporting
{

    public class GameObjectChildrenRule : AssetRelationRuleBase
    {

        public GameObjectChildrenRule() : base(typeof(GameObject))
        { }

        protected override List<Object> TryFindRelatedAssets(Object unityObject)
        {
            var result = base.TryFindRelatedAssets(unityObject);

            var gameObject = unityObject as GameObject;
            foreach(Transform child in gameObject.transform)
                result.Add(child.gameObject);

            return result;
        }

    }

}
