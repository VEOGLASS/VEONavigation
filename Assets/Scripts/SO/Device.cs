using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XploriaAR
{
    [CreateAssetMenu(menuName = "XploriaAR/DeviceData", order = 1)]
    public class Device : ScriptableObject
    {
        [SerializeField]
        private string deviceName;
        [SerializeField]
        private List<string> keyWords = new List<string>();

        public void SetName(string name)
        {
            deviceName = name;
        }

        public string GetName(string name)
        {
            return deviceName;
        }

        public void AddKeyWord(string keyWord)
        {
            if (!keyWords.Contains(keyWord)) keyWords.Add(keyWord);
        }

        public bool RemoveKeyWord(string keyWord)
        {
            return keyWords.Remove(keyWord);
        }

        public bool IsUsed()
        {
            var value = keyWords.Exists(word => SystemInfo.deviceModel.Contains(word) || SystemInfo.deviceName.Contains(word));
#if UNITY_ANDROID
            if (value) Android.ShowAndroidToastMessage(deviceName + " recognized");
#endif
            return value;
        }
    }
}