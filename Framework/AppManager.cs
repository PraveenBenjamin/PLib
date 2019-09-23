using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PLib.Declarations;
using UnityEngine.Events;

namespace PLib.Framework
{
    public class AppManager : SingletonBehaviour<AppManager>
    {
        public enum AppStates
        {
            Foreground,

            //neither of the 2 states below are supported for now
            Windowed,
            Background
        }


        //think of a way to protect this, will preserving the design
        [SerializeField]
        private Canvas _uiRoot;
        public Canvas UIRoot
        {
            get
            {
                return _uiRoot;
            }
        }


        private FSM<AppStates> _appManagerFSM;

        private UnityAction _temporaryVariableMaintainence;

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            _appManagerFSM = new FSM<AppStates>();
            _appManagerFSM.Initialize(this,Constants._frameworkFsmInitPrefix, Constants._frameworkFsmUpdatePrefix, Constants._frameworkFsmTerminatePrefix);
            _appManagerFSM.SetState(AppStates.Foreground);

            _temporaryVariableMaintainence = TemporaryVariableManager.GetMaintainenceDelegate();
        }


        private void InitForeground()
        {
            //first launch
            if (GameManager.Instance == null)
            {
                //create menumanager
                GameObject temp = new GameObject("GameManager");
                GameManager gm = temp.AddComponent<GameManager>();
                gm.InitializeGameManager();
                gm.OnAppStateChange(AppStates.Foreground);
                gm.transform.SetParent(this.transform,false);
            }
        }

        private void UpdateForeground()
        {
            GameManager.Instance.UpdateGameManager();
            _temporaryVariableMaintainence?.Invoke();
        }

        public void Update()
        {
            //Hopefully, the only update that will be used throughout the program
            _appManagerFSM.UpdateStateMachine();
        }

#if !UNITY_EDITOR

        private void OnApplicationFocus(bool focus)
        {
            GameManager.Instance.OnAppStateChange(focus ? AppStates.Foreground : AppStates.Background);
        }

        private void OnApplicationQuit()
        {
            GameManager.Instance.DestroyGameManager();
        }
#endif

    }
}
