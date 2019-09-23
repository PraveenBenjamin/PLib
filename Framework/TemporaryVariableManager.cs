using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace PLib.Framework
{
    /// <summary>
    /// Creates and exposes temporary variables to any class that might need it.
    /// Made to be a central system that allows for easy disposal of temporary variables, instead of having the clutter up other classes
    /// </summary>
    public static class TemporaryVariableManager
    {
        public static Dictionary<UnityEngine.Object, Dictionary<int, object>> _temporaryVariables = new Dictionary<UnityEngine.Object, Dictionary<int, object>>();

        public static UnityAction GetMaintainenceDelegate()
        {
            Debug.Log("GetMaintainenceDelegate called, ensure it isnt called unnecessarily, for performance reasons. Recommendation:- once per update.");
            return PerformMaintainence;
        }

        private static Dictionary<int, object> GetDictionary(UnityEngine.Object instanceBoundTo, int index)
        {
            Dictionary<int, object> dic = null;
            if (!_temporaryVariables.ContainsKey(instanceBoundTo))
                _temporaryVariables.Add(instanceBoundTo, new Dictionary<int, object>());


            dic = _temporaryVariables[instanceBoundTo];

            return dic;
        }


        /// <summary>
        /// Retrieves a previously stored temporary variable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instanceBoundTo"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T GetTemporaryVariable<T>(UnityEngine.Object instanceBoundTo, int index)
        {
            Dictionary<int, object> dic = GetDictionary(instanceBoundTo, index);

            if (dic.ContainsKey(index))
            {
                //dic.Add(index, (T)Activator.CreateInstance(typeof(T)));
                return (T)dic[index];
            }
            return default(T);

        }

        /// <summary>
        /// Sets a previously stored temporary variable
        /// or force adds it if the parameter is specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instanceBoundTo"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="forceAdd"></param>
        /// <returns></returns>
        public static bool SetTemporaryVariable<T>(UnityEngine.Object instanceBoundTo, int index, T value, bool forceAdd = false)
        {

            Dictionary<int, object> dic = GetDictionary(instanceBoundTo, index);

            if (dic.ContainsKey(index))
                dic[index] = (object)value;
            else if (forceAdd)
                dic.Add(index, value);

            return dic.ContainsKey(index);

        }


        /// <summary>
        /// runs maintainence on internal dictionaries
        /// </summary>
        private static void PerformMaintainence()
        {

            bool hasNullOuterKeys = false;

            foreach (KeyValuePair<UnityEngine.Object, Dictionary<int, object>> pairOuter in _temporaryVariables)
            {

                if (pairOuter.Key == null)
                {
                    hasNullOuterKeys = true;
                    continue;
                }

                List<int> badKeysInner = pairOuter.Value.Where(pair => pair.Value == null)
                        .Select(pair => pair.Key)
                        .ToList();

                if (badKeysInner == null || badKeysInner.Count > 0)
                    continue;
                
                foreach (var badKeyInner in badKeysInner)
                {
                    pairOuter.Value.Remove(badKeyInner);
                }
            }


            if (!hasNullOuterKeys)
                return;


            var badKeys = _temporaryVariables.Where(pair => pair.Key == null || pair.Value.Count == 0)
                        .Select(pair => pair.Key)
                        .ToList();
            foreach (var badKey in badKeys)
            {
                _temporaryVariables.Remove(badKey);
            }

        }
    }

}