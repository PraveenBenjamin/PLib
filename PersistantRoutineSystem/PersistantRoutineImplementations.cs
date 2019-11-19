using System.Reflection;

namespace SSL_Framework.PersistantRoutineSystem
{

    /// <summary>
    /// Class that exposes the various feilds that an IPersistantRoutine may require to the editor 
    /// </summary>
    [System.Serializable]
    public sealed class PersistantRoutineDatum
    {
        public string Routine;
        public string[] ScenesToInvokeOn;
        public bool InvokeAfterEverySceneLoad = false;
        public int BlindInvocationCount = -1;
    }


    /// <summary>
    /// An IPersistantRoutine that will be invoked once when the next scene is loaded, then invalidate itself
    /// </summary>
    public sealed class BlindOneShotPersistantRoutine : IPersistantRoutine
    {
        private MethodInfo _routineToInvoke;

        public bool IsValid
        {
            get
            {
                return _routineToInvoke != null;
            }
        }

        public void AssignRoutine(MethodInfo routine)
        {
            _routineToInvoke = routine;
        }

        public void Invoke(string name)
        {
            _routineToInvoke?.Invoke(null,null);
            _routineToInvoke = null;
        }
    }


    /// <summary>
    /// an IPersistantRoutine that will invoke itself once when a particular scene is loaded, then invalidate itself
    /// </summary>
    public sealed class OneShotPersistantRoutine : IPersistantRoutine
    {
        private string _sceneToInvokeOn;
        private MethodInfo _routineToInvoke;
        private object[] _params;


        public void AssignRoutine(MethodInfo routine)
        {
            _routineToInvoke = routine;
        }

        public void Initialize(string sceneToInvokeOn)
        {
            _sceneToInvokeOn = sceneToInvokeOn;
        }

        public bool IsValid
        {
            get
            {
                if (_routineToInvoke == null || !OnSceneLoadedPersistantRoutineManager.Instance.CheckIfSceneIsInBuild(_sceneToInvokeOn))
                {
                    return false;
                }
                return true;
            }
        }

        public void Invoke(string sceneName)
        {
            if (!sceneName.Equals(_sceneToInvokeOn))
                return;

            _routineToInvoke?.Invoke(null, _params);
            _routineToInvoke = null;
        }
    }
}