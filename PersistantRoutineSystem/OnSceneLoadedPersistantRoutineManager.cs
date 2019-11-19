using SSL_Framework;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SSL_Framework.PersistantRoutineSystem
{

    /// <summary>
    /// Interface that PersistantRoutine implementations must implement
    /// </summary>
    public interface IPersistantRoutine
    {
        /// <summary>
        /// Assign an instance of a function to this IPersitantRoutine
        /// </summary>
        /// <param name="routine"></param>
        /// <param name="parameters"></param>
        void AssignRoutine(MethodInfo routine);

        //will be destroyed by the manager if this returns false OnSceneLoaded
        bool IsValid { get; }

        void Invoke(string name);
    }





    /// <summary>
    /// Invokes persistant routines provided to it through its interface onSceneLoaded
    /// </summary>
    public sealed class OnSceneLoadedPersistantRoutineManager : Singleton<OnSceneLoadedPersistantRoutineManager>
    {
        [SerializeField]
        private HashSet<string> _allScenesInBuild = null;
        [SerializeField]
        private List<IPersistantRoutine> _routines = null;
        [SerializeField]
        private TextAsset _runtimeCodeTemplate;
        [SerializeField]
        private int _routineUID;



        public bool IsInitializedAndReady
        {
            get
            {
                return !(_allScenesInBuild == null || _allScenesInBuild.Count <= 0);
            }
        }

        protected override void InitializeSingleton()
        {
            //https://answers.unity.com/questions/1128694/how-can-i-get-a-list-of-all-scenes-in-the-build.html
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            _allScenesInBuild = new HashSet<string>();
            for (int i = 0; i < sceneCount; i++)
            {
                _allScenesInBuild.Add(Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i)));
            }

            _routines = new List<IPersistantRoutine>();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        public bool CheckIfSceneIsInBuild(string sceneName)
        {
            //why you do this user?
            if (System.String.IsNullOrEmpty(sceneName))
                return false;

            if (!IsInitializedAndReady)
                return false;

            return _allScenesInBuild.Contains(sceneName);
        }

        /// <summary>
        /// Enqueue the persistant routine provided as the parameter
        /// OnSceneLoaded, Call the implementation of Invoke exposed by the IPersistantRoutine interface
        /// Then remove the routine from the list of active routines
        /// The "Enqueue" part of the name is to let the user know that the invocations will happen in the order they were provided.
        /// </summary>
        /// <param name="routine"></param>
        public void EnqueuePersistantRoutine(IPersistantRoutine routine)
        {
            if (_routines == null)
            {
                Debug.LogError("PersistantRoutineManager not initialized yet. Please wait until its Awake function is Invoked before Adding a routine");
                return;
            }

            _routines?.Add(routine);
        }


        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_routines == null || _routines.Count <= 0)
                return;

            for (int i = 0; i < _routines.Count; ++i)
            {
                _routines[i].Invoke(scene.name);
            }

            _routines.RemoveAll((x) => !x.IsValid);
        }


        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }


    }
}

