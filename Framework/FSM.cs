using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.Events;

namespace PLib.Framework
{
    /// <summary>
    /// Abstract Finite state machine
    /// Calls methods through reflection based on the state and state of the state (init, update or terminate)
    /// Prefixes to decipher methods to call via c# reflection exist in POP.Framework.Constants
    /// </summary>
    /// <typeparam name="T">enum to generate states with</typeparam>
    public class FSM<T> where T : struct, IConvertible
    {
        public class State
        {

            public enum StateComponent
            {
                Init,
                Update,
                Terminate
            }


            private T _baseEnumValue;
            private TypeInfo _typeofState;
            private UnityAction _initCallback;
            private UnityAction _updateCallback;
            private UnityAction _terminateCallback;

            public State()
            {
                _initCallback = null;
                _updateCallback = null;
                _terminateCallback = null;
            }

            /// <summary>
            /// Initializes the state
            /// Uses reflection to identify which methods to call within to the instance sent as parameter
            /// Sets initial state
            /// </summary>
            /// <param name="enumValue"> enum to bind state to </param>
            /// <param name="toInitializeWith"> object to bind state to </param>
            /// <param name="initPrefix">prefix used to identify initCallback</param>
            /// <param name="updatePrefix">prefix used to identify updateCallback</param>
            /// <param name="terminatePrefix">prefix used to identify terminateCallback</param>
            public void Initialize(T enumValue, UnityEngine.Object toInitializeWith, string initPrefix, string updatePrefix, string terminatePrefix)
            {
                _baseEnumValue = enumValue;
                _typeofState = toInitializeWith.GetType().GetTypeInfo();

                MethodInfo[] allMethods = toInitializeWith.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


                //use reflection to auto assign required callbacks
                MethodInfo curr;
                string methodNameToConsider;
                for (int i = 0; i < allMethods.Length; ++i)
                {
                    curr = allMethods[i];
                    methodNameToConsider = initPrefix + _baseEnumValue.ToString();
                    //methodNameToConsider.Insert(0, initPrefix);
                    if (curr.Name.Equals(methodNameToConsider))
                    {
                        _initCallback = (UnityAction)curr.CreateDelegate(typeof(UnityAction), toInitializeWith);
                    }

                    methodNameToConsider = updatePrefix + _baseEnumValue.ToString();
                    //methodNameToConsider.Insert(0, updatePrefix);
                    if (curr.Name.Equals(methodNameToConsider))
                    {
                        _updateCallback = (UnityAction)curr.CreateDelegate(typeof(UnityAction), toInitializeWith);
                    }

                    methodNameToConsider = terminatePrefix + _baseEnumValue.ToString();
                    //methodNameToConsider.Insert(0, terminatePrefix);
                    if (curr.Name.Equals(methodNameToConsider))
                    {
                        _terminateCallback = (UnityAction)curr.CreateDelegate(typeof(UnityAction), toInitializeWith);
                    }
                }

                if (_initCallback == null && _updateCallback == null && _terminateCallback == null)
                {
                    //Debug.Log("empty state detected, was this intended?" + _baseEnumValue);
                }
            }

            /// <summary>
            /// Invokes callbacks if present
            /// </summary>
            /// <param name="comp"> state of state (init, update or terminate)</param>
            public void Invoke(StateComponent comp)
            {
                switch (comp)
                {
                    case StateComponent.Init:
                        _initCallback?.Invoke();
                        break;
                    case StateComponent.Update:
                        _updateCallback?.Invoke();
                        break;
                    case StateComponent.Terminate:
                        _terminateCallback?.Invoke();
                        break;
                }
            }

            ~State()
            {
                _initCallback = null;
                _terminateCallback = null;
                _updateCallback = null;
            }
        }


        private State _currentState;
        private Nullable<T> _currentStateIndex;
        private Nullable<T>[] _nextStatesIndices = new Nullable<T>[2];
        private Nullable<T> _prevStateIndex;

        private T[] _enumValsCache;
        private State[] _states;


        /// <summary>
        /// Initializes the statemachine by generating states and hooking up the callbacks that exist in the parameter, identified through reflection
        /// </summary>
        /// <param name="instanceToInitializeFor">the object this state machine is intended for</param>
        /// <returns></returns>
        public bool Initialize(UnityEngine.Object instanceToInitializeFor, string initPrefix, string updatePrefix, string terminatePrefix)
        {

            if (instanceToInitializeFor == null)
            {
                Logger.Log(this,Logger.Level.Critical,"instance is Null, why you do dis?");
                return false;
            }

            //refresh
            _currentState = null;
            _currentStateIndex = null;
            System.Array.Clear(_nextStatesIndices, 0, _nextStatesIndices.Length);
            _nextStatesIndices = new Nullable<T>[2];
            _prevStateIndex = null;

            try
            {
                _enumValsCache = (T[])System.Enum.GetValues(typeof(T));

                _states = new State[_enumValsCache.Length];
                for (int i = 0; i < _enumValsCache.Length; ++i)
                {
                    State toCreate = new State();
                    toCreate.Initialize((T)_enumValsCache.GetValue(i), instanceToInitializeFor, initPrefix, updatePrefix, terminatePrefix);
                    _states[i] = toCreate;
                }


                _currentStateIndex = (T)_enumValsCache.GetValue(0);

                return true;
            }
            catch (Exception e)
            {
                Logger.Log(this, Logger.Level.Critical, "Initialization of FSM failed for object: " + instanceToInitializeFor.GetType() + instanceToInitializeFor.name + " with error: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates fields that changes this machines state in the next update cycle
        /// </summary>
        /// <param name="toSet"></param>
        public void SetState(T toSet)
        {
            if (toSet.Equals(_currentStateIndex))
                return;

            if (_nextStatesIndices[0].Equals(toSet))
                return;

            if (_nextStatesIndices[0].Equals(null))
            {
                _nextStatesIndices[0] = toSet;
                return;
            }


            _nextStatesIndices[1] = toSet;

        }

        public T GetState()
        {
            return (T)_currentStateIndex;
        }


        private bool CheckAndChangeState()
        {

            //first launch
            if (_currentState == null)
            {
                _currentState = _states[System.Convert.ToInt32(_currentStateIndex.Value)];
                _currentState.Invoke(State.StateComponent.Init);
                return true;
            }

            //if the user wants to switch states
            if (_nextStatesIndices[0] != null)
            {
                //terminate current
                _currentState.Invoke(State.StateComponent.Terminate);

                //switch index to new state
                _currentStateIndex = _nextStatesIndices[0];
                _nextStatesIndices[0] = _nextStatesIndices[1];
                _nextStatesIndices[1] = null;

                //since T is always expected to be an enum, we can assume this wont fail (the check happens in the Initialize method)
                //assign state associated with index of current state
                _currentState = _states[System.Convert.ToInt32(_currentStateIndex.Value)];

                //initialize the next (now set to current) state
                _currentState.Invoke(State.StateComponent.Init);

                return true;
            }
            return false;
        }

        /// <summary>
        /// updates the statemachine and invokes the appropriate callbacks based on the state
        /// </summary>
        public void UpdateStateMachine()
        {
            if (_currentStateIndex == null)
                return;

            //if a state change didnt happen this cycle
            if (!CheckAndChangeState())
            {

                //update the state
                _currentState.Invoke(State.StateComponent.Update);
            }
        }

        private void ReleaseResources()
        {
            //invoke destructor within the states
            System.Array.Clear(_states, 0, _states.Length);

            //clear erething!
            _states = null;
            _enumValsCache = null;
        }

        ~FSM()
        {
            ReleaseResources();
        }
    }
}