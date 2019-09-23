using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PLib.Framework
{

    /// <summary>
    /// Singleton behaviour abstract class.
    /// Extend to create singletons within the application
    /// entry point for singleton updation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : class
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                return _instance;
            }
        }

        public static Y GetInstance<Y>() where Y:T
        {
            return (Y)System.Convert.ChangeType(_instance,typeof(Y));
        }


        protected virtual void InitializeSingleton() { }
        protected virtual void OnDestroySingleton() { }
        protected virtual void UpdateSingleton() { }

        private void Awake()
        {
            if (_instance != null)
            {
                Debug.LogWarning("Attempting to create a second instance of a SingletonBehaviour. This is not allowed. Destroying component" + this.GetType().ToString());
                DestroyImmediate(this);
                return;
            }
            _instance = (T)System.Convert.ChangeType(this, typeof(T));
            InitializeSingleton();
        }

        protected void Update()
        {
            UpdateSingleton();
        }

        private void OnDestroy()
        {
            _instance = null;
            OnDestroySingleton();
        }
    }
}
