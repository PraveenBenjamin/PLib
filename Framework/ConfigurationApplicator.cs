using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PLib.Framework;
using PLib.Declarations;
using System.Reflection;
using System;

namespace PLib.Declarations
{

    [System.Serializable]
    public class ConfigurationData
    {
        public IntConfigurationDatumDict Data;
    }


    [System.Serializable]
    public class ConfigurationDatum
    {
        [System.Serializable]
        public class ConfigurationPropertyInfoContainer
        {
            public string PropertyName = null;
            public string PropertyType = null;
            public string ValueToSet = null;
        }

        public string GameObjectHierarchy = null;
        public string TypeOfComponent = null;

        [SerializeField]
        public List<ConfigurationPropertyInfoContainer> PropertyInfo;

    }
}

namespace PLib.Framework
{

    //Gonna make this a singleton monobehaviour for now. It is intended to be a static class that loads config data from external sources eventually.
    public class ConfigurationApplicator : SingletonBehaviour<ConfigurationApplicator>
    {

        [SerializeField]
        StringConfigurationDataDict _configDataDict;

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
        }

        public void ApplyConfiguration(System.Type type, GameObject ob, int configVariant = 0)
        {
            //sanity
            if (ob == null)
            {
                Logger.Log(this, Logger.Level.Critical, "Object to bind to is null or empty. Why you do this?");
                return;
            }

            if (!_configDataDict.ContainsKey(type.ToString()))
            {
                Logger.Log(this, Logger.Level.Critical, "Configuration datum doest exist for type " + type.ToString());
                return;
            }

            string key = type.ToString();

            ConfigurationData data = _configDataDict[key];
            if (data == null || data.Data == null || data.Data.Count == 0)
            {
                Logger.Log(this, Logger.Level.Critical, "Configuration data is empty for type " + type.ToString());
                return;
            }

            if (!data.Data.ContainsKey(configVariant))
            {
                Logger.Log(this, Logger.Level.Critical, "Configuration datum for type " + type.ToString() + " and variant " + configVariant + " cannot be found.");
                return;
            }

            ConfigurationDatum datum = data.Data[configVariant];

            if (datum == null)
            {
                Logger.Log(this, Logger.Level.Critical, "Configuration datum for type " + type.ToString() + " and variant " + configVariant + " is null or empty.");
                return;
            }

            Transform goToModify = ob.transform.Find(datum.GameObjectHierarchy);
            if (goToModify == null)
            {
                Logger.Log(this, Logger.Level.Critical, "cannot find child at hierarchy " + datum.GameObjectHierarchy);
                return;
            }

            Component compToModify = goToModify.GetComponent(type);
            if (compToModify == null)
            {
                Logger.Log(this, Logger.Level.Critical, "cannot find component of type " + datum.TypeOfComponent + " at hierarchy " + datum.GameObjectHierarchy);
                return;
            }

            ApplyConfigurationInternal(compToModify, datum);
        }


        private static object GetValueAsObject(string type, string value)
        {
            object toSet = value;

            switch (type)
            {
                case "System.Int32":
                    toSet = int.Parse(value);
                    break;
                case "System.Single":
                    toSet = float.Parse(value);
                    break;
            }

            return toSet;
        }

        private static void ApplyConfigurationInternal(Component compToModify, ConfigurationDatum datum)
        {
            PropertyInfo[] allProps = compToModify.GetType().GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);

            for (int datumPropIndex = 0; datumPropIndex < datum.PropertyInfo.Count; ++datumPropIndex)
            {
                ConfigurationDatum.ConfigurationPropertyInfoContainer propConfigInfo = datum.PropertyInfo[datumPropIndex];
                if (propConfigInfo == null || propConfigInfo.PropertyName == null || propConfigInfo.PropertyName.Length == 0)
                    continue;

                PropertyInfo inf;
                for (int obPropIndex = 0; obPropIndex < allProps.Length; ++obPropIndex)
                {
                    inf = allProps[obPropIndex];
                    if (inf.Name.CompareTo(propConfigInfo.PropertyName) != 0)
                        continue;

                    if (inf.PropertyType.ToString() != propConfigInfo.PropertyType)
                        continue;

                    object toSet = GetValueAsObject(propConfigInfo.PropertyType, propConfigInfo.ValueToSet);

                    inf.SetValue(compToModify,toSet );
                }
            }
        }
    }

}