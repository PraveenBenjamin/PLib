using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PLib.Declarations;
using Newtonsoft.Json;
using System.Linq;

namespace PLib.Framework
{

    [System.Serializable]
    public class Language
    {
        public StringStringDict data;
    }


    public class LanguageManager : SingletonBehaviour<LanguageManager>
    {

        [SerializeField]
        StringLanguageDict _languageData;

        private string _languageToUse;

        private string _languageJSONAssetPath { get; set; }


        protected override void InitializeSingleton()
        {
            _languageData = new StringLanguageDict();
            ConfigurationApplicator.Instance.ApplyConfiguration(this.GetType(), this.gameObject, 0);
            InitializeLanguageData();
        }

        private void InitializeLanguageData()
        {
            string data = Resources.Load<TextAsset>(_languageJSONAssetPath).ToString();

            _languageData = JsonConvert.DeserializeObject<StringLanguageDict>(data);

            SetLanguage(_languageData.Keys.ToList()[0]);
        }


        public void SetLanguage(string languageKey)
        {
            if (_languageData.Keys.Contains(languageKey))
                _languageToUse = languageKey;
            else
                Logger.Log(this, Logger.Level.Verbose, "Unable to switch language, invalid key: " + languageKey);
        }

        public string GetLanguageElement(string key)
        {
            StringStringDict d = _languageData[_languageToUse].data;
            if (d.ContainsKey(key))
                return d[key];

            Logger.Log(this, Logger.Level.Verbose, "Couldnt find language element for key: " + key);
            return "";
        }

    }
}
