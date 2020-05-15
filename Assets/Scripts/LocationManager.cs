using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace XploriaAR
{
    using XploriaAR.Network;

    public class LocationManager : MonoBehaviour
    {
        private IHandleInput inputHandler;

        #region Inspector fields

#pragma warning disable 649
        [SerializeField]
        private bool useExternalGps = true;
#pragma warning restore 649

        [Space]

        [SerializeField, ConditionalField("useExternalGps", true)]
        private BluetoothServer externalReceiver;

        [Space]

        [SerializeField, Tooltip("Accuracy(in meters)."), ConditionalField("useExternalGps", false)]
        private float gpsAccuracy = 1;
        [SerializeField, Tooltip("GPS update distance(in meters) >= 0.1m.")]
        private float updateDistance = 1;
        [SerializeField, Tooltip("Update interval(in seconds) >= 0.5s.")]
        private float updateInterval = 1;

        [Space, Separator]

        //[SerializeField, ReadOnly]
        //private Vector2 calculatedDevicePoint;
        //[SerializeField, ReadOnly]
        //private Vector2 calculatedStartPoint;
        //[SerializeField, ReadOnly]
        //private Vector2 calculatedPreviousPoint;

        //[Space]

        [SerializeField]
        private Location deviceLocation;
        [SerializeField, HideInInspector]
        private Location startLocation;
        [SerializeField]
        private Location previousLocation;

        [Space]

        [SerializeField]
        private LocationEvent onLocationChange;

        #endregion


        private void Awake()
        {
            #region Self-Injection
#if UNITY_EDITOR
            inputHandler = new UnityInputHandler();
#else
            inputHandler = new UnityInputHandler();
#endif
            #endregion
        }

        private void Start()
        {
            StartLocationService();
        }

        private void OnValidate()
        {
            //clamping distances and time
            gpsAccuracy = Mathf.Max(gpsAccuracy, 0.1f);
            updateDistance = Mathf.Max(updateDistance, 0.1f);
            updateInterval = Mathf.Max(updateInterval, 0.5f);

            //calcalute device coor in x and y
            //calculatedStartPoint = Scholar.GPSCoorToPoint(startLocation);
            //calculatedDevicePoint = Scholar.GPSCoorToPoint(deviceLocation);
            //calculatedPreviousPoint = Scholar.GPSCoorToPoint(previousLocation);
        }


        private void StartLocationService()
        {
            StartCoroutine(LocationServices());
        }

        private IEnumerator LocationServices()
        {
            StartLocation = DeviceLocation;

            if (useExternalGps)
            {
                externalReceiver?.AddOnDataReceivedListener((data) =>
                {
                    PreviousLocation = DeviceLocation;
                    DeviceLocation = JsonUtility.FromJson<NavigationApiResponse>(data).data.location;
                });
                yield break;
            }

            /*
             * 
             * 
             * 
             * 
             */

            {
                //setting desired update interval
                var interval = new WaitForSeconds(updateInterval);
                if (!inputHandler.LocationService.isEnabledByUser)
                {
#if UNITY_ANDROID
                    Android.ShowAndroidToastMessage("Enable GPS", 4000);
#endif
                    yield return new WaitWhile(() => !inputHandler.LocationService.isEnabledByUser);
                }

                //initializing location serivce with desired accuracy
                inputHandler.LocationService.Start(gpsAccuracy, updateDistance);

                while (inputHandler.LocationService.status == LocationServiceStatus.Initializing)
                {
                    yield return interval;
                }

                //checking final status
                if (inputHandler.LocationService.status == LocationServiceStatus.Failed)
                {
#if UNITY_ANDROID
                    Android.ShowAndroidToastMessage("Unable to determine device location", 4000);
#endif
                    yield break;
                }
                else
                {
                    //current device location update
                    while (true)
                    {
                        PreviousLocation = DeviceLocation;
                        DeviceLocation = new Location(inputHandler.LocationService.lastData);
                        yield return interval;
                    }
                }
            }
        }


        public void AddOnLocationChangeListener(UnityAction<Location> listener)
        {
            if (onLocationChange == null) onLocationChange = new LocationEvent();
            onLocationChange.AddListener(listener);
        }

        public void RemoveOnLocationChangeListener(UnityAction<Location> listener)
        {
            onLocationChange?.RemoveListener(listener);
        }

        public void RemoveAllOnLocationChangeListeners()
        {
            onLocationChange?.RemoveAllListeners();
        }


        public bool UseExternalGps
        {
            get => useExternalGps; 
            set => useExternalGps = value; 
        }

        //public Vector2 CalculatedStartPoint => calculatedStartPoint;
        //public Vector2 CalculatedDevicePoint => calculatedDevicePoint;

        public Location StartLocation
        {
            get => startLocation; 
            private set
            {
                startLocation = value;
                //calculatedStartPoint = Scholar.GPSCoorToPoint(value);
            }
        }

        public Location DeviceLocation
        {
            get => deviceLocation;
            private set
            {
                deviceLocation = value;
                onLocationChange?.Invoke(value);
                //calculatedDevicePoint = Scholar.GPSCoorToPoint(value);
            }
        }

        public Location PreviousLocation
        {
            get => previousLocation;
            private set
            {
                previousLocation = value;
                //calculatedPreviousPoint = Scholar.GPSCoorToPoint(value);
            }
        }

        public BluetoothServer ExternalReceiver
        {
            get => externalReceiver; 
            set => externalReceiver = value;
        }
    }
}