using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace XploriaAR.Network
{
    using SVSBluetooth;

    public class BluetoothServer : MonoBehaviour
    {
        public GameObject deviceStatus;

        [SerializeField]
        private StringEvent onDataReceived;
        public float interval = 1.0f;
        private const string UUID = "8b10d196-4efd-4d83-a942-63fc970e591b";

        private void Start()
        {
            BluetoothForAndroid.Initialize();
            if (BluetoothForAndroid.IsBTEnabled())
            {
                CreateServer();
            }
            else
            {
                BluetoothForAndroid.EnableBT();
            }
        }

        private void OnEnable()
        {
            BluetoothForAndroid.BtAdapterEnabled += OnBtAdapterEnabled;
            BluetoothForAndroid.ServerStarted += OnServerStarted;
            BluetoothForAndroid.ServerStopped += OnServerStopped;
            BluetoothForAndroid.DeviceDisconnected += OnDeviceDisconnected;
            BluetoothForAndroid.DeviceConnected += OnDeviceConnected;
        }

        private void OnDisable()
        {
            BluetoothForAndroid.BtAdapterEnabled -= OnBtAdapterEnabled;
            BluetoothForAndroid.ServerStarted -= OnServerStarted;
            BluetoothForAndroid.ServerStopped -= OnServerStopped;
            BluetoothForAndroid.DeviceDisconnected -= OnDeviceDisconnected;
            BluetoothForAndroid.DeviceConnected -= OnDeviceConnected;
        }


        private void OnBtAdapterEnabled()
        {
            CreateServer();
        }

        private void OnServerStarted()
        {
            BluetoothForAndroid.ReceivedStringMessage += GetMessage;
            Debug.Log("Server started");
        }

        private void OnServerStopped()
        { 
            Debug.Log("Server stopped");
        }

        private void GetMessage(string data)
        {
            Debug.Log("Received data: " + data);
            onDataReceived?.Invoke(data);
        }

        private void OnDeviceDisconnected()
        {
            deviceStatus?.gameObject.SetActive(true);
            CreateServer();
        }

        private void OnDeviceConnected()
        {
            deviceStatus?.gameObject.SetActive(false);
        }


        private void OnApplicationQuit()
        {
            Disconnect();
        }


        public void CreateServer()
        {
            BluetoothForAndroid.CreateServer(UUID);
        }

        public void Disconnect()
        {
            BluetoothForAndroid.ReceivedStringMessage -= GetMessage;
            BluetoothForAndroid.Disconnect();
        }

        public void AddOnDataReceivedListener(UnityAction<string> listener)
        {
            if (onDataReceived == null) onDataReceived = new StringEvent();
            onDataReceived.AddListener(listener);
        }

        public void RemoveOnDataReceivedListener(UnityAction<string> listener)
        {
            onDataReceived?.RemoveListener(listener);
        }
    }
}