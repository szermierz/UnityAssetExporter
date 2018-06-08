using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetsExporting
{

    public class SerializedObjectReferencesRule : AssetRelationRuleBase
    {
        public SerializedObjectReferencesRule() : base(typeof(UnityEngine.Object))
        { }

        protected override List<UnityEngine.Object> TryFindRelatedAssets(UnityEngine.Object unityObject)
        {
            var result = base.TryFindRelatedAssets(unityObject);

            SerializedObject serializedObject = new SerializedObject(unityObject);

            var property = serializedObject.GetIterator();
            while (property.NextVisible(true))
                TryAddSerializedProperty(property, result);

            return result;
        }

        protected virtual void TryAddSerializedProperty(SerializedProperty property, List<UnityEngine.Object> result)
        {
            if(null == property || null == result)
                return;
            
            switch(property.propertyType)
            {
            case SerializedPropertyType.ObjectReference:
                result.Add(property.objectReferenceValue);
                break;
            case SerializedPropertyType.ExposedReference:
                result.Add(property.exposedReferenceValue);
                break;
            default:
                break;
            }
        }
        
    }

}
