using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using AssetsExportingHelpers;

namespace AssetsExportingHelpers
{

    public static class ReflectiveClassSearch
    {

        public static IEnumerable<T> CreateInheritedClassInstances<T>(params object[] constructorArgs) where T : class
        {
            var types = Assembly.GetAssembly(typeof(T)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)));

            foreach(Type type in types)
                yield return (T)Activator.CreateInstance(type, constructorArgs);
        }

    }

}

namespace AssetsExporting
{

    public class AssetRelationBuildingVisitor
    {

        public AssetRelationBuildingVisitor(bool autoCollectRules = true)
        {
            if(autoCollectRules)
                AddAllRelationRules();
        }

        public AssetRelationRuleBase.ObjectMatchedDelegate OnObjectEntered;
        public AssetRelationRuleBase.ObjectMatchedDelegate OnObjectFound;

        public virtual void Build(UnityEngine.Object unityObject)
        {
            OnObjectFound -= EnterObject;
            OnObjectFound += EnterObject;

            OnObjectFound.Invoke(unityObject);
        }

        protected virtual void EnterObject(UnityEngine.Object unityObject)
        {
            if(null != OnObjectEntered)
                OnObjectEntered.Invoke(unityObject);

            foreach(var ruleEntry in m_Rules)
            {
                var rule = ruleEntry.Value;
                rule.VerifyObject(unityObject);
            }
        }

        protected Dictionary<Type, AssetRelationRuleBase> m_Rules = new Dictionary<Type, AssetRelationRuleBase>();

        public virtual bool AddRelationRule(AssetRelationRuleBase rule)
        {
            if(m_Rules.ContainsKey(rule.GetType()))
                return false;

            m_Rules.Add(rule.GetType(), rule);

            rule.OnObjectMatched -= OnObjectFound;
            rule.OnObjectMatched += OnObjectFound;

            return true;
        }

        protected virtual bool AddAllRelationRules()
        {
            bool result = false;

            foreach(var rule in ReflectiveClassSearch.CreateInheritedClassInstances<AssetRelationRuleBase>())
                result |= AddRelationRule(rule);

            return result;
        }

    }

}