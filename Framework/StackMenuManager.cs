using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLib.Framework
{
    /// <summary>
    /// Manages the menu stack of the game by pushing and popping game menus as instructed through its exposed functions
    /// </summary>
    public class StackMenuManager : SingletonBehaviour<StackMenuManager>
    {
        //this list will be treated like a stack. Hence the name of the variable
        private List<BaseMenu> _menus;


        public T InstantiateMenuInstance<T>() where T : BaseMenu
        {
            GameObject newMen = AssetManager.Instance.GetPrefabInstance(typeof(T).ToString());
            T men = newMen.GetComponent<T>();
            return men;
        }

        /// <summary>
        /// pushes a basemenu to the stack
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onComplete"></param>
        public T PushMenu<T>(int configurationVariant = 0,bool skipTransition = false, UnityAction<BaseMenu> onMenuConstructed = null, UnityAction<BaseMenu> onTransitionComplete = null) where T : BaseMenu
        {
            BaseMenu men = InstantiateMenuInstance<T>();
            men.transform.SetParent(AppManager.Instance.UIRoot.transform, false);
            men.Construct(configurationVariant, onMenuConstructed);
            _menus.Add(men);
            men.TransitionIn(skipTransition,onTransitionComplete);
            return (T)men;
        }


        /// <summary>
        /// Transitions out the menu at the top of the stack, then destroys it
        /// </summary>
        /// <param name="onComplete"></param>
        public void PopMenu(UnityAction<BaseMenu> onTransitionOutComplete = null, bool skipTransition = false, UnityAction<BaseMenu> onDestructionComplete = null)
        {
            BaseMenu men = PeekMenu<BaseMenu>();
            men.TransitionOut(skipTransition,(BaseMenu m1) =>
            {
                onTransitionOutComplete?.Invoke(m1);
                men.Destruct((BaseMenu m2) =>
                {
                    onDestructionComplete?.Invoke(m2);
                    PopMenu();
                });
            });
        }

        /// <summary>
        /// returns a reference to the menu at the top of the stack
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T PeekMenu<T>() where T : BaseMenu
        {
            return (T)_menus[_menus.Count - 1];
        }

        private void PopMenu()
        {
            if(_menus.Count > 0)
                _menus.RemoveAt(_menus.Count - 1);
        }


        protected override void InitializeSingleton()
        {
            _menus = new List<BaseMenu>();
        }

        protected override void OnDestroySingleton()
        {
            if (_menus != null && _menus.Count > 0)
            {
                foreach (BaseMenu men in _menus)
                {
                    men.Destruct();
                }

                _menus.Clear();
                _menus = null;
            }
        }

        /// <summary>
        /// updates the menus' state machines
        /// </summary>
        public void UpdateMenuManager()
        {

            //Update dem menus!
            for (int i = 0; i < _menus.Count; ++i)
            {
                _menus[i].UpdateMenu();
            }

        }
    }
}
