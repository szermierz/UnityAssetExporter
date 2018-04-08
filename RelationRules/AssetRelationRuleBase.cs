﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetRelationRuleBase
{

    public readonly Type MatchingObjectType;

    public AssetRelationRuleBase(Type matchingObjectType)
    {
        MatchingObjectType = matchingObjectType;
    }

    public delegate void ObjectMatchedDelegate(UnityEngine.Object unityObject);

    public ObjectMatchedDelegate OnObjectMatched;

    /* Template method design pattern */
    public virtual bool VerifyObject(UnityEngine.Object unityObject)
    {
        if(!VeryfyObjectType(unityObject))
            return false;

        var relatedAssets = TryFindRelatedAssets(unityObject);=
        NotifyRelationFound(relatedAssets);

        return true;
    }

    protected virtual void NotifyRelationFound(List<UnityEngine.Object> relatedAssets)
    {
        if(null == relatedAssets || null == OnObjectMatched)
            return;

        foreach(var relatedAsset in relatedAssets)
            OnObjectMatched.Invoke(relatedAsset);
    }

    protected virtual bool VeryfyObjectType(UnityEngine.Object unityObject)
    {
        return MatchingObjectType.IsAssignableFrom(unityObject.GetType());
    }

    protected virtual List<UnityEngine.Object> TryFindRelatedAssets(UnityEngine.Object unityObject)
    {
        return new List<UnityEngine.Object>();
    }

}