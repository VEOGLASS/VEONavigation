using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace XploriaAR
{
    using WearHFPlugin;

    /// <summary>
    /// Voice command manager, useable only on the RealWear devices. Requires WearHF component.
    /// </summary>
    [RequireComponent(typeof(WearHF))]
    public class CommandManager : MonoBehaviour
    {
        // NOTE:
        // There is unkown bug when after random amount of time WearHF component stops working.
        // Workaround: setting and resetting commands in provided interval ;(
        private const float resetInterval = 5.0f;

        [Serializable]
        public struct VoiceCommand
        {
            public string text;
            public UnityEvent action;

            public VoiceCommand(string text, UnityEvent action)
            {
                this.text = text;
                this.action = action;
            }
        }


        private WearHF wearHf;

        [SerializeField]
        private List<VoiceCommand> commands = new List<VoiceCommand>();


        private void Awake()
        {
            wearHf = GetComponent<WearHF>();
        }

        private void Start()
        {
            StartCoroutine(WearHfLiveLoop());
        }

        private IEnumerator WearHfLiveLoop()
        {
            var interval = new WaitForSecondsRealtime(resetInterval);
            while (true)
            {
                wearHf.ClearCommands();
                foreach (var command in commands)
                {
                    InitWearHfCommand(command);
                }

                wearHf.EnableActionButton = false;
                wearHf.EnableGlobalCommands = false;
                yield return interval;
            }
        }


        private void InitWearHfCommand(VoiceCommand command)
        {
            wearHf.AddVoiceCommand(command.text, (text) =>
            {              
                command.action?.Invoke();
            });
        }


        public void AddCommand(string command, UnityEvent action)
        {
            AddCommand(new VoiceCommand(command, action));
        }

        public void AddCommand(VoiceCommand command)
        {
            InitWearHfCommand(command);
            commands.Add(command);
        }

        public void RemoveCommand(string command, UnityEvent action)
        {
            RemoveCommand(new VoiceCommand(command, action));
        }

        public void RemoveCommand(VoiceCommand command)
        {
            wearHf.RemoveVoiceCommand(command.text);
            commands.Remove(command);
        }
    }
}