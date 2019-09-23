using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PLib.Framework;

namespace PLib.Framework
{

    public partial class GameManager : SingletonBehaviour<GameManager>,IGameManager
    {
        partial void InitializeGameManagerInternal();
        partial void OnAppStateChangeInternal(AppManager.AppStates state);
        partial void UpdateGameManagerInternal();

        public void InitializeGameManager()
        {
            InitializeGameManagerInternal();
        }

        public void OnAppStateChange(AppManager.AppStates state)
        {
            OnAppStateChangeInternal(state);
        }

        public void UpdateGameManager()
        {
            UpdateGameManagerInternal();
        }
    }

    public interface IGameManager
    {

        void InitializeGameManager();

        void OnAppStateChange(AppManager.AppStates state);

        void UpdateGameManager();
    }

}