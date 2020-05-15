using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace XploriaAR.Network
{
    using SVSBluetooth;

    public class BluetoothClient : MonoBehaviour
    {
        private const string UUID = "8b10d196-4efd-4d83-a942-63fc970e591b";

        public StringEvent onDataReceived;
        public Text sendedText;
        public float interval = 1.0f;

        public LocationManager locationManager;


        private void Start()
        {
            BluetoothForAndroid.Initialize();
            if (BluetoothForAndroid.IsBTEnabled())
            {
                ConnectToServer();
            }
            else
            {
                BluetoothForAndroid.EnableBT();
            }
        }

        private void OnEnable()
        {
            BluetoothForAndroid.BtAdapterEnabled += OnBtAdapterEnabled;
            BluetoothForAndroid.DeviceConnected += OnDeviceConnected;
            BluetoothForAndroid.FailConnectToServer += OnFailConnectToServer;
            BluetoothForAndroid.DeviceDisconnected += OnDeviceDisconnected;
        }

        private void OnDisable()
        {
            BluetoothForAndroid.BtAdapterEnabled -= OnBtAdapterEnabled;
            BluetoothForAndroid.DeviceConnected -= OnDeviceConnected;
            BluetoothForAndroid.FailConnectToServer -= OnFailConnectToServer;
            BluetoothForAndroid.DeviceDisconnected -= OnDeviceDisconnected;
        }

        private void OnFailConnectToServer()
        {
            if (BluetoothForAndroid.IsBTEnabled())
            {
                Debug.Log("Bluetooth is enabled, trying to connect again...");
                ConnectToServer();
            }
            else
            {
                Debug.Log("Trying to enable bluetooth");
                BluetoothForAndroid.EnableBT();
            }
        }

        private void OnBtAdapterEnabled()
        {
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            BluetoothForAndroid.ConnectToServer(UUID);
        }

        private void OnDeviceConnected()
        {
            InvokeRepeating("DataCreator", 0.1f, interval);
        }

        private void OnDeviceDisconnected()
        {
            CancelInvoke("DataCreator");
            ConnectToServer();
        }


        public void DataCreator()
        {
            var nmeaData = new NMEAData()
            {
                windDirection = UnityEngine.Random.Range(60.0f, 120.0f),
                windStrength = UnityEngine.Random.Range(0, 100),
                currentCourse = 154,
                desiredCourse = 101,
                location = locationManager.DeviceLocation
            };
            var navigationApiResponse = new NavigationApiResponse()
            {
                data = nmeaData
            };
            
            var data = JsonUtility.ToJson(navigationApiResponse);
            sendedText.text = data;
            BluetoothForAndroid.WriteMessage(data);
        }
    }
}
