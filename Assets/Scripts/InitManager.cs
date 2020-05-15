using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace XploriaAR
{
    [DisallowMultipleComponent, RequireComponent(typeof(SceneManager))]
    public class InitManager : MonoBehaviour
    {
        [Serializable]
        public struct DeviceScene
        {
            public string name;
            public Device[] devices;

            public DeviceScene(string sceneName, Device[] devices)
            {
                this.name = sceneName;
                this.devices = devices;
            }

            public bool AnyDeviceUsed()
            {
                return Array.Exists(devices, device => device.IsUsed());
            }
        }

        private SceneManager sceneManager;

        [SerializeField]
        private bool forceScene;
        [SerializeField, Tooltip("Scene name used in forcing mode.")]
        private string desiredSceneName;

        [SerializeField, ReadOnly]
        private DeviceScene defaultScene;

        [SerializeField]
        private List<DeviceScene> availableScenes = new List<DeviceScene>();

        [Space]

        [SerializeField]
        private UnityEvent onInitEnd;


        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            sceneManager = GetComponent<SceneManager>();

            if (onInitEnd == null)
            {
                onInitEnd = new UnityEvent();
            }

            onInitEnd.AddListener(() =>
            {
                if (forceScene)
                {
                    sceneManager.LoadScene(desiredSceneName);
                    return;
                }

                foreach (var scene in availableScenes)
                {
                    if (!scene.AnyDeviceUsed()) continue;
                    sceneManager.LoadScene(scene.name);
                    return;
                }
                sceneManager.LoadScene(defaultScene.name);
            });
        }

        private void Start()
        {
            StartCoroutine(Initialization());
        }

        private void Reset()
        {
            defaultScene = new DeviceScene("MenuScene", new Device[] { Resources.Load<Device>("Devices/Default") });
        }

        private void OnValidate()
        {
            defaultScene = new DeviceScene("MenuScene", new Device[] { Resources.Load<Device>("Devices/Default") });
        }

        private IEnumerator Initialization()
        {
#if UNITY_ANDROID
            //checking needed permissions
            if (!Android.HasCameraPermission)
            {
                yield return Android.GetCameraAccess();
            }

            //do not start proccess without camera permission
            yield return new WaitWhile(() => !Android.HasCameraPermission);

            //if (!Android.HasLocationPermission)
            //{
            //    yield return Android.GetLoactionAccess();
            //}

            //yield return new WaitWhile(() => !Android.HasLocationPermission);
#endif
            onInitEnd?.Invoke();
        }


        public void AddDevice(DeviceScene device)
        {
            availableScenes.Add(device);
        }

        public void RemoveDevice(DeviceScene device)
        {
            availableScenes.Remove(device);
        }

        public void AddOnInitEndListener(UnityAction call)
        {
            if (onInitEnd == null) onInitEnd = new UnityEvent();
            onInitEnd.AddListener(call);
        }

        public void RemoveOnInitEndListener(UnityAction call)
        {
            onInitEnd?.RemoveListener(call);
        }


        public bool ForceScene
        {
            get { return forceScene; }
            set { forceScene = value; }
        }

        public string DesiredSceneName
        {
            get { return desiredSceneName; }
            set { desiredSceneName = value; }
        }
    }
}