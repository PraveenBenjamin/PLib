using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PLib.Framework;
using PLib.Declarations;


namespace PLib.Framework
{
    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        //ideally i would want to load this through the configuration. But for now it will be part of the scene
        [SerializeField]
        private StringAudioClipDict _audioDatabase;
        //for bg music and other peristant things!
        private Dictionary<string, AudioSource> _persistantAudioSources;

        //through config
        private string _oneShotAudioSourceKey { get; set; }




        protected override void InitializeSingleton()
        {
            ConfigurationApplicator.Instance.ApplyConfiguration(this.GetType(), this.gameObject, 0);
            _persistantAudioSources = new Dictionary<string, AudioSource>();

        }

        private AudioClip GetClip(string key)
        {
            if (_audioDatabase.ContainsKey(key))
                return _audioDatabase[key];
            return null;
        }

        private AudioSource GetSource(string key, bool forceAdd = false)
        {
            if (!_persistantAudioSources.ContainsKey(key) && forceAdd)
                _persistantAudioSources.Add(key, this.gameObject.AddComponent<AudioSource>());

            if (_persistantAudioSources.ContainsKey(key))
                return _persistantAudioSources[key];

            return null;
        }

        public void PlayOneShot(string key)
        {
            AudioClip c = GetClip(key);
            if (c == null)
                return;

            GetSource(_oneShotAudioSourceKey, true)?.PlayOneShot(GetClip(key));
        }

        public void PlayPersistant(string key)
        {
            AudioClip c = GetClip(key);
            if (c == null)
                return;

            AudioSource src = GetSource(key, true);
            src.clip = c;
            src.loop = true;
            src.Play();
        }

        public void PausePersistant(string key, bool pause = true)
        {
            AudioSource src = GetSource(key);
            if (src == null)
                return;

            if (pause)
                src.Pause();
            else
                src.UnPause();
        }

        public void StopPersistant(string key)
        {
            AudioSource src = GetSource(key);
            if (src == null)
                return;

            src.Stop();
            _persistantAudioSources.Remove(key);
            Destroy(src);

        }

        public void SetVolume(float nVolume)
        {
            foreach (KeyValuePair<string, AudioSource> pair in _persistantAudioSources)
            {
                pair.Value.volume = nVolume;
            }
        }

    }
}