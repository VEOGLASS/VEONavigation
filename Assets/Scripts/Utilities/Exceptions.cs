using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace XploriaAR.Exceptions
{
    public class ErrorMessageException : Exception
    {
        public ErrorMessageException(string message = "Error type message received.") : base(message)
        {
#if UNITY_EDITOR
            Debug.LogError(message);
#endif
            if (Application.platform != RuntimePlatform.Android) return;
            Android.ShowAndroidToastMessage(message, 3500);
        }
    }

    public class ApiDataFormatException : Exception
    {
        public ApiDataFormatException(string message = "Wrong QR code data format!") : base(message)
        {
#if UNITY_EDITOR
            Debug.LogError(message);
#endif
            if (Application.platform != RuntimePlatform.Android) return;
            Android.ShowAndroidToastMessage(message, 3500);
        }
    }

    public class UrlFormatException : Exception
    {
        public UrlFormatException(string message = "Cannot read URL!") : base(message)
        {
#if UNITY_EDITOR
            Debug.LogError(message);
#endif
            if (Application.platform != RuntimePlatform.Android) return;
            Android.ShowAndroidToastMessage(message, 3500);
        }
    }

    public class GuidFormatException : Exception
    {
        public GuidFormatException(string message = "Cannot read access token!") : base(message)
        {
#if UNITY_EDITOR
            Debug.LogError(message);
#endif
            if (Application.platform != RuntimePlatform.Android) return;
            Android.ShowAndroidToastMessage(message, 3500);
        }
    }
}