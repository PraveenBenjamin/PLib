using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PLib.Declarations;

namespace PLib.Framework
{

    /// <summary>
    /// Base class of all menus that will be generated and used in the application
    /// </summary>
    [RequireComponent(typeof(UnityEngine.RectTransform))]
    public abstract class BaseMenu : MonoBehaviour
    {

        public enum BaseMenuStates
        {
            Construction,
            TransitionIn,
            Main,
            TransitionOut,
            Disabled,
            Destruction,
            Destroyed
        }

        protected FSM<BaseMenuStates> _baseMenuFSM;

        private int _onTransitionInCBIndex = 0;
        private int _onTransitionOutCBIndex = 1;

        private int _onConstructionCompleteCBIndex = 2;
        private int _onDestructionCompleteCBIndex = 3;


        /// <summary>
        /// updates the menu FSM and calls InputIndependantUpdateRoutine and InputDependantUpdateRoutine internally
        /// </summary>
        public void UpdateMenu()
        {
            _baseMenuFSM.UpdateStateMachine();
        }


        /// <summary>
        /// Construction routine. 
        /// </summary>
        //dont like the fact that this has to be public, unfortunately c# has no chill for friends :/ :p
        public void Construct(int configurationVariant = 0, UnityAction<BaseMenu> onComplete = null)
        {
            _baseMenuFSM = new FSM<BaseMenuStates>();
            _baseMenuFSM.Initialize(this,Constants._frameworkFsmInitPrefix,Constants._frameworkFsmUpdatePrefix,Constants._frameworkFsmTerminatePrefix);

            TemporaryVariableManager.SetTemporaryVariable<UnityAction<BaseMenu>>(this, _onConstructionCompleteCBIndex, onComplete, true);

            ConfigurationApplicator.Instance.ApplyConfiguration(this.GetType(), this.gameObject, configurationVariant);

            _baseMenuFSM.SetState(BaseMenuStates.Construction);
            _baseMenuFSM.UpdateStateMachine();
        }


        protected virtual void InitConstruction()
        {
            //if the user doesnt want to use this state, i.e user doesnt want to control the construction phase, set state to idle
            _baseMenuFSM.SetState(BaseMenuStates.Main);
        }

        protected void TerminateConstruction()
        {
            TerminateConstructionInternal();
            UnityAction<BaseMenu> cb = TemporaryVariableManager.GetTemporaryVariable<UnityAction<BaseMenu>>(this, _onConstructionCompleteCBIndex);
            cb?.Invoke(this);
        }

        protected virtual void TerminateConstructionInternal() { }


        public virtual void Destruct(UnityAction<BaseMenu> onComplete = null)
        {
            TemporaryVariableManager.SetTemporaryVariable<UnityAction<BaseMenu>>(this, _onDestructionCompleteCBIndex, onComplete, true);
            _baseMenuFSM.SetState(BaseMenuStates.Destruction);
        }

        protected void InitDestruction()
        {
            _baseMenuFSM.SetState(BaseMenuStates.Destroyed);
        }

        protected void TerminateDestruction()
        {
            TerminateDestructionInternal();
            UnityAction<BaseMenu> cb = TemporaryVariableManager.GetTemporaryVariable<UnityAction<BaseMenu>>(this, _onDestructionCompleteCBIndex);
            cb?.Invoke(this);

            // I mean 0 offense when i say this, buuuuuuut....
            // ALLAHU AKBAR!
            GameObject.Destroy(this.gameObject);
        }

        protected virtual void TerminateDestructionInternal() { }


        protected abstract void OnSkipTransition(BaseMenuStates transitionToSkip);



        /// <summary>
        /// Sets menu state to transitioning in
        /// </summary>
        /// <param name="onComplete"></param>
        public virtual void TransitionIn(bool skipTransition = false,UnityAction<BaseMenu> onComplete = null)
        {
            TemporaryVariableManager.SetTemporaryVariable<UnityAction<BaseMenu>>(this, _onTransitionInCBIndex, onComplete, true);
            if (skipTransition)
            {
                OnSkipTransition(BaseMenuStates.TransitionIn);
                onComplete?.Invoke(this);
                _baseMenuFSM.SetState(BaseMenuStates.Main);
                return;
            }
            _baseMenuFSM.SetState(BaseMenuStates.TransitionIn);
        }


        protected void TerminateTransitionIn()
        {
            TerminateTransitionInInternal();
            UnityAction<BaseMenu> cb = TemporaryVariableManager.GetTemporaryVariable<UnityAction<BaseMenu>>(this, _onTransitionInCBIndex);
            cb?.Invoke(this);
        }

        protected virtual void TerminateTransitionInInternal() { }

        /// <summary>
        /// sets menu state to transitioning out
        /// </summary>
        /// <param name="onComplete"></param>
        public virtual void TransitionOut(bool skipTransition = false,UnityAction<BaseMenu> onComplete = null)
        {
            TemporaryVariableManager.SetTemporaryVariable<UnityAction<BaseMenu>>(this, _onTransitionOutCBIndex, onComplete, true);
            if (skipTransition)
            {
                OnSkipTransition(BaseMenuStates.TransitionOut);
                onComplete?.Invoke(this);
                _baseMenuFSM.SetState(BaseMenuStates.Disabled);
                return;
            }
            _baseMenuFSM.SetState(BaseMenuStates.TransitionOut);
        }


        protected void TerminateTransitionOut()
        {
            TerminateTransitionOutInternal();
            UnityAction<BaseMenu> cb = TemporaryVariableManager.GetTemporaryVariable<UnityAction<BaseMenu>>(this, _onTransitionOutCBIndex);
            cb?.Invoke(this);
            _baseMenuFSM.SetState(BaseMenuStates.Disabled);
        }

        protected virtual void TerminateTransitionOutInternal() { }

    }
}