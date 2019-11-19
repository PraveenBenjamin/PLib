using UnityEngine;

namespace SSL_Framework.PersistantRoutineSystem
{

    //Initializes persistant routines on awake, assigns them on enable, then kills itself
    public sealed class AssignPersistantRoutinesOnEnable : MonoBehaviour
    {

        [SerializeField]
        private PersistantRoutineDatum[] RoutinesToEnqueue;

        private IPersistantRoutine[] _initializedRoutines;

        public void Awake()
        {
            _initializedRoutines = new IPersistantRoutine[RoutinesToEnqueue.Length];
            IPersistantRoutine r = null;
            for (int i = 0; i < RoutinesToEnqueue.Length; ++i)
            {
                r = OnSceneLoadedPersistantRoutineFactory.GetPersistantRoutine(RoutinesToEnqueue[i]);
                _initializedRoutines[i] = r;
            }
        }


        private void OnEnable()
        {
            if (RoutinesToEnqueue != null && RoutinesToEnqueue.Length > 0)
            {
                for (int i = 0; i < _initializedRoutines.Length; ++i)
                {
                    //r = OnSceneLoadedPersistantRoutineFactory.GetPersistantRoutine(RoutinesToEnqueue[i]);
                    OnSceneLoadedPersistantRoutineManager.Instance.EnqueuePersistantRoutine(_initializedRoutines[i]);
                }

            }

            Destroy(this);
        }
    }
}
