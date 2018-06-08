using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AssetsExporting
{

    public class GameObjectComponentsRule : AssetRelationRuleBase
    {

        public GameObjectComponentsRule() : base(typeof(GameObject))
        { }

        protected override List<Object> TryFindRelatedAssets(Object unityObject)
        {
            var result = base.TryFindRelatedAssets(unityObject);

            var gameObject = unityObject as GameObject;
            if (!gameObject)
                return result;

            var components = gameObject.GetComponents<Component>();
            result.AddRange(components);

            return result;
        }

    }

}
