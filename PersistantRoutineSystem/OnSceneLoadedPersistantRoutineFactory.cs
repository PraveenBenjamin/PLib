using System.Reflection;
using UnityEngine;

namespace SSL_Framework.PersistantRoutineSystem
{

    public enum PersistantRoutineType
    {
        None,
        //play once, requires scene name
        OneShot,
        //play once, no scene name required
        BlindOneShot,

        //the ones below have not yet been implemented

        //play multiple times, multiple scene names required
        MultipleInvocations,

        //always play until manually removed after every scene load
        BlindPersistant,

        //play a set number of times
        BlindMultipleInvocations

    }


    /// <summary>
    /// Creates PersistantRoutine instances and returns IPersistantRoutine handles.
    /// </summary>
    public static class OnSceneLoadedPersistantRoutineFactory
    {


        //Infers the type of the persistant routine that matches best with the contents of the PersistantRoutineDatum provided as the parameter
        private static PersistantRoutineType InferType(PersistantRoutineDatum dat)
        {
            if (dat == null)
                return PersistantRoutineType.None;

            //does not have a scene to invoke on
            if (dat.ScenesToInvokeOn == null || dat.ScenesToInvokeOn.Length == 0)
            {
                if (dat.InvokeAfterEverySceneLoad)
                    return PersistantRoutineType.BlindPersistant;
                else if (dat.BlindInvocationCount > 0)
                    return PersistantRoutineType.BlindMultipleInvocations;
                return PersistantRoutineType.BlindOneShot;
            }

            if (dat.ScenesToInvokeOn.Length == 1)
            {
                return PersistantRoutineType.OneShot;
            }

            if (dat.ScenesToInvokeOn.Length > 1)
            {
                return PersistantRoutineType.MultipleInvocations;
            }

            return PersistantRoutineType.None;
        }


        /// <summary>
        /// Infers and creates a persistant routine based on the datum provided as parameter
        /// </summary>
        /// <param name="dat"></param>
        /// <returns></returns>
        public static IPersistantRoutine GetPersistantRoutine(PersistantRoutineDatum dat)
        {
            return GetPersistantRoutine(InferType(dat), SSL_Utils.PersistantRoutineCompiler.CreateRoutine(dat), dat.ScenesToInvokeOn);
        }

        private static IPersistantRoutine GetPersistantRoutine(PersistantRoutineType type, MethodInfo toInvoke, params string[] _scenesToInvokeOn)
        {
            IPersistantRoutine toReturn = null;
            switch (type)
            {
                case PersistantRoutineType.OneShot:
                    {
                        if (_scenesToInvokeOn == null || _scenesToInvokeOn.Length == 0)
                        {
                            Debug.LogWarning("Cannot create a OneShot routine if no scene name is specified");
                        }
                        else
                        {
                            OneShotPersistantRoutine routine = new OneShotPersistantRoutine();
                            routine.Initialize(_scenesToInvokeOn[0]);
                            toReturn = routine;
                        }
                    }
                    break;
                case PersistantRoutineType.BlindOneShot:
                    {
                        BlindOneShotPersistantRoutine routine = new BlindOneShotPersistantRoutine();
                        toReturn = routine;
                    }
                    break;
                default:
                    {
                        throw new System.NotImplementedException();
                    }
            }

            toReturn?.AssignRoutine(toInvoke);

            if (!toReturn.IsValid)
                Debug.Log("Invalid PersistantRoutine. It will not invoke.");


            return toReturn;
        }

    }
}
