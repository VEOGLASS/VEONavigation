using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Android;

namespace XploriaAR
{
    public static class Android
    {
        private static class AndroidCameraParameters
        {
            public readonly static AndroidJavaObject camera;
            public readonly static AndroidJavaObject parameters;
     
            public readonly static int numberOfCameras;

            public readonly static float focalLength;
            public readonly static float verticalViewAngle;
            public readonly static float horizontalViewAngle;

            public readonly static Vector2 sensorSize;

            static AndroidCameraParameters()
            {
                if (Application.platform != RuntimePlatform.Android) return;

                AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");

                numberOfCameras = cameraClass.CallStatic<int>("getNumberOfCameras");

                try
                {
                    camera = cameraClass.CallStatic<AndroidJavaObject>("open", 0);
                }
                catch (Exception e)
                {
                    Debug.LogError("[CameraParametersAndroid]" + e);
                }

                parameters = camera.Call<AndroidJavaObject>("getParameters");

                focalLength = parameters.Call<float>("getFocalLength");
                verticalViewAngle = parameters.Call<float>("getVerticalViewAngle");
                horizontalViewAngle = parameters.Call<float>("getHorizontalViewAngle");

                sensorSize = new Vector2
                {
                    //TODO:
                    x = Mathf.Tan(horizontalViewAngle / 2) * focalLength,
                    y = Mathf.Tan(verticalViewAngle / 2) * focalLength
                };
            }
        }


        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        /// <param name="duration">Duration in miliseconds.</param>
        public static void ShowAndroidToastMessage(string message, int duration = 2000)
        {
            if (Application.platform != RuntimePlatform.Android) return;

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, duration);
                    toastObject.Call("show");
                }));
            }
        }

        /// <summary>
        /// Return current Android keyboard height.
        /// </summary>
        /// <returns></returns>
        public static float GetKeyboardHeightRatio()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return 0.4f; //fake TouchScreenKeyboard height ratio for debug in editor        
            }

#if UNITY_ANDROID
            using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity")
                    .Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");

                using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
                {
                    View.Call("getWindowVisibleDisplayFrame", rect);
                    return (float)(Screen.height - rect.Call<int>("height")) / Screen.height;
                }
            }
#else
            return (float)TouchScreenKeyboard.area.height / Screen.height;
#endif
        }

        /// <summary>
        /// Sends to Android Device the camera permission request. Method based on <see cref="Permission"/> class.
        /// </summary>
        /// <param name="timeToWait">Desired offset time to app response.</param>
        /// <returns></returns>
        public static IEnumerator GetCameraAccess(float timeToWait = 2.0f)
        {
            if (Application.platform != RuntimePlatform.Android) yield break;

            var processTime = 0.0f;

            Permission.RequestUserPermission(Permission.Camera);
            while (processTime < timeToWait)
            {
                if (HasCameraPermission)
                {
                    yield break;
                }
                processTime += Time.unscaledDeltaTime;
                yield return null;
            }

            ShowAndroidToastMessage("Appliacation requires camera access to proceed");
            yield return new WaitForSeconds(timeToWait);
            Application.Quit();
        }

        /// <summary>
        /// Sends to Android Device the coarse location permission request. Method based on <see cref="Permission"/> class.
        /// </summary>
        /// <param name="timeToWait">Desired offset time to app response.</param>
        /// <returns></returns>
        public static IEnumerator GetLoactionAccess(float timeToWait = 2.0f)
        {
            if (Application.platform != RuntimePlatform.Android) yield break;

            var processTime = 0.0f;

            Permission.RequestUserPermission(Permission.FineLocation);
            while (processTime < timeToWait)
            {
                if (HasLocationPermission)
                {
                    yield break;
                }
                processTime += Time.unscaledDeltaTime;
                yield return null;
            }

            ShowAndroidToastMessage("Appliacation requires GPS location access to proceed");
            yield return new WaitForSeconds(timeToWait);
            Application.Quit();
        }


        public static bool HasCameraPermission => Permission.HasUserAuthorizedPermission(Permission.Camera);
        public static bool HasLocationPermission => Permission.HasUserAuthorizedPermission(Permission.FineLocation);

        public static float FocalLength => AndroidCameraParameters.focalLength;
        public static float VerticalViewAngle => AndroidCameraParameters.verticalViewAngle;
        public static float HorizontalViewAngle => AndroidCameraParameters.horizontalViewAngle;

        public static Vector2 SensorSize => AndroidCameraParameters.sensorSize;
    }
}