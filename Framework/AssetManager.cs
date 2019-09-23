using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PLib.Framework;
using PLib.Declarations;

/// <summary>
/// kinda a stub, dont really know where to take this given the usual simplicity of the applications we make
/// It just instantiates prefabs for now.
/// </summary>
public class AssetManager : SingletonBehaviour<AssetManager>
{

    [SerializeField]
    private StringGameObjectDict _runtimePrefabDictionary;

    public GameObject GetPrefabInstance(string key)
    {
        if (!_runtimePrefabDictionary.ContainsKey(key))
        {
            Logger.Log(this, Logger.Level.Critical, "Unable to find prefab instance for key: " + key);
            return null;
        }
        GameObject go = GameObject.Instantiate(_runtimePrefabDictionary[key]);
        go.name = go.name.Replace(Constants._unityPrefabSuffix, "").Trim();
        return go;
        
    }
}
