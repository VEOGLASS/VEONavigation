using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace XploriaAR
{
    public class InputManager : MonoBehaviour
    {
        [Serializable]
        public struct Shortcut
        {
            public string shortcutName;
            public KeyCode[] keyCodes;
            public UnityEvent keyEvent;

            public Shortcut(string shortcutName, UnityEvent keyEvent, KeyCode[] keyCodes)
            {
                this.shortcutName = shortcutName;
                this.keyCodes = keyCodes;
                this.keyEvent = keyEvent;
            }
        }

        public struct PrioritizedEvent
        {
            public float prority;
            public UnityAction action;
            public Func<bool> condition;

            public PrioritizedEvent(float prority)
            {
                this.prority = prority;
                this.action = () => { };
                this.condition = () => true;
            }

            public PrioritizedEvent(float prority, UnityAction action, Func<bool> condition)
            {
                this.prority = prority;
                this.action = action;
                this.condition = condition;
            }
        }

        private IHandleInput input;

        private Dictionary<KeyCode[], UnityEvent> basicShortcuts;

        #region Inspector fields 

        [SerializeField]
        private UnityEvent anyKeyEvent;

#pragma warning disable 649
        /// <summary>
        /// Inspector shortcuts wrapper.
        /// </summary>
        [SerializeField]
        private List<Shortcut> shortcouts;
#pragma warning restore 649

        #endregion

        private void Awake()
        {
            #region Self-Injection

            input = new UnityInputHandler();

            #endregion

            basicShortcuts = new Dictionary<KeyCode[], UnityEvent>();

            shortcouts.ForEach(sc => AddListener(sc.keyEvent.Invoke, sc.keyCodes));
        }

        private void Update()
        {
            if (input.AnyKey) anyKeyEvent?.Invoke();

            foreach (var entry in basicShortcuts)
            {
                var pressed = true;
                for (int i = 0; i < entry.Key.Length - 1; i++)
                {
                    if (!input.GetKey(entry.Key[i]))
                    {
                        pressed = false;
                        break;
                    }
                }

                if (!pressed) continue;

                if (input.GetKeyDown(entry.Key[entry.Key.Length - 1]))
                {
                    entry.Value?.Invoke();
                }
            }
        }

        public void AddAnyListener(UnityAction listener)
        {
            if (anyKeyEvent == null) anyKeyEvent = new UnityEvent();
            anyKeyEvent.AddListener(listener);
        }

        public void AddListener(UnityAction listener, params KeyCode[] key)
        {
            if (basicShortcuts.ContainsKey(key))
            {
                basicShortcuts[key].AddListener(listener);
            }
            else
            {
                var e = new UnityEvent();
                e.AddListener(listener);
                basicShortcuts.Add(key, e);
            }
        }

        public void RemoveAnyListener(UnityAction listener)
        {
            anyKeyEvent?.RemoveListener(listener);
        }

        public void RemoveListener(UnityAction listener, params KeyCode[] key)
        {
            if (basicShortcuts.ContainsKey(key))
            {
                basicShortcuts[key].RemoveListener(listener);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("Invalid key(s)!", this);
#endif
            }
        }
    }
}