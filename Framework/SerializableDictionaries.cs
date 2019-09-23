using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using PLib.Framework;

namespace PLib.Declarations
{
    //string keys
    [System.Serializable]
    public class StringStringDict : SerializableDictionaryBase<string, string> { }

    [System.Serializable]
    public class StringGameObjectDict : SerializableDictionaryBase<string, GameObject> { }

    [System.Serializable]
    public class StringConfigurationDataDict : SerializableDictionaryBase<string, ConfigurationData> { }

    [System.Serializable]
    public class StringAudioClipDict : SerializableDictionaryBase<string, AudioClip> { }

    [System.Serializable]
    public class StringLanguageDict : SerializableDictionaryBase<string, Language> { }



    //int keys
    [System.Serializable]
    public class IntConfigurationDatumDict : SerializableDictionaryBase<int, ConfigurationDatum> { }


}