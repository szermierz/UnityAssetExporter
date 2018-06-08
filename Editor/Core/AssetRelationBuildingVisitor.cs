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

        protected Queue<UnityEngine.Object> m_ObjectsToEnterPool = new Queue<UnityEngine.Object>();
        protected HashSet<UnityEngine.Object> m_AlreadyEnteredObjects = new HashSet<UnityEngine.Object>();

        public virtual void Build(UnityEngine.Object unityObject)
        {
            Build(Enumerable.Repeat(unityObject, 1));
        }

        public virtual void Build(IEnumerable<UnityEngine.Object> unityObjects)
        {
            OnObjectFound -= QueueObject;
            OnObjectFound += QueueObject;

            m_AlreadyEnteredObjects.Clear();

            QueueObject(unityObjects);

            while(EnterNextQueueObject())
                ;
        }

        protected virtual bool EnterNextQueueObject()
        {
            if(!m_ObjectsToEnterPool.Any())
                return false;

            var objectToEnter = m_ObjectsToEnterPool.Dequeue();

            if(null == objectToEnter)
                return true;

            EnterObject(objectToEnter);

            return true;
        }

        protected virtual void QueueObject(UnityEngine.Object unityObject)
        {
            QueueObject(Enumerable.Repeat(unityObject, 1));
        }

        protected virtual void QueueObject(IEnumerable<UnityEngine.Object> unityObjects)
        {
            foreach(var unityObject in unityObjects)
                if(!m_AlreadyEnteredObjects.Contains(unityObject))
                    m_ObjectsToEnterPool.Enqueue(unityObject);
        }

        protected virtual void EnterObject(UnityEngine.Object unityObject)
        {
            if (m_AlreadyEnteredObjects.Contains(unityObject))
                return;

            m_AlreadyEnteredObjects.Add(unityObject);

            if (null != OnObjectEntered)
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

            rule.OnObjectMatched -= OnRuleFoundObject;
            rule.OnObjectMatched += OnRuleFoundObject;

            return true;
        }

        public virtual int ExcludeRelationRulesOfType(Type ruleType)
        {
            return ExcludeRelationRules(x => x.GetType() == ruleType);
        }

        public virtual int ExcludeRelationRules(System.Func<AssetRelationRuleBase, bool> verifier)
        {
            int previousSize = m_Rules.Count;

            m_Rules = m_Rules.Where(x => verifier(x.Value)).ToDictionary(x => x.Key, x => x.Value);

            return previousSize - m_Rules.Count;
        }

        protected virtual void OnRuleFoundObject(UnityEngine.Object unityObject)
        {
            if(null != OnObjectFound)
                OnObjectFound.Invoke(unityObject);
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