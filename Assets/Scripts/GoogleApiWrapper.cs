using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Networking;

namespace XploriaAR
{
    [Obsolete("Use LocationManager and AppManager instead.")]
    public class GoogleApiWrapper : MonoBehaviour
    {
        #region Private fields

        private IHandleInput inputHandler;

        private readonly List<PlaceRenderer> knownTargets = new List<PlaceRenderer>();
        private readonly List<GoogleRoute> lastUsedRoutes = new List<GoogleRoute>();

        #endregion

        #region Inspector fields

        [SerializeField, Multiline]
        private string apiKey = "AIzaSyBhKgyaouRTotZT_yFgNiZ1GhgJbylUVv0";

        [SerializeField]
        private bool searchRoutes = true;
        [SerializeField]
        private bool searchPlaces = true;

        [Separator, Header("Rendering")]

        [SerializeField]
        private Transform worldRoot;
        [SerializeField]
        private PlaceRenderer worldTargetPrefab;
        [SerializeField]
        private LineRenderer worldRouteRenderer;

        [Separator, Header("Measurement")]

        [SerializeField]
        private bool useExternalGps = true;

        [Space]

        [SerializeField, ConditionalField("useExternalGps", true)]
        private Network.NetworkServer externalReceiver;

        [Space]

        [SerializeField, Tooltip("Accuracy(in meters)."), ConditionalField("useExternalGps", false)]
        private float gpsAccuracy = 1;
        [SerializeField, Tooltip("Search radius(in meters)."), ReadOnly]
        private float searchRadius = 50;
        [SerializeField, Tooltip("Device radius(in meters), used to ignore too close targets."), ReadOnly]
        private float deviceRadius = 5;
        [SerializeField, Tooltip("GPS update distance(in meters).")]
        private float updateDistance = 1;
        [SerializeField, Tooltip("Update interval(in seconds) >= 1s.")]
        private float updateInterval = 1;

        [Space]

        [SerializeField, ReadOnly]
        private Vector2 calculatedDevicePoint;
        [SerializeField, ReadOnly]
        private Vector2 calculatedTargetPoint;
        [SerializeField, ReadOnly]
        private Vector2 calculatedStartPoint;

        [Space]

        [SerializeField]
        private Location deviceLocation;
        [SerializeField]
        private Location targetLocation;
        [SerializeField, HideInInspector]
        private Location startLocation;

        [Separator, Space]

        [SerializeField]
        private GoogleTravelMode travelMode;

        [Space]

        [SerializeField, Tooltip("Provided types will be excluded from instantiation.")]
        private List<string> typesToExclude = new List<string>() { "locality", "route" };

        [Space]

        [SerializeField]
        private List<GooglePlace> predefinedPlaces = new List<GooglePlace>();

        [Separator, Header("Debug")]

#pragma warning disable 414
        [SerializeField, Tooltip("Conditional field used in debugging.")]
        private bool enableDebug = true;
#pragma warning restore 414
        [Tooltip("File name + type.")]
        [SerializeField, ConditionalField("enableDebug")]
        private string dataFileName = "data.json";

        #endregion

        #region Init methods

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
            var filePath = Application.persistentDataPath + "/" + dataFileName;
            if (File.Exists(filePath))
            {
                var data = File.ReadAllText(filePath);
                if (data == null) return;
                //var response = JsonUtility.FromJson<GoogleDirectionsApiResponse>(data);
                var response = JsonUtility.FromJson<GooglePlacesApiResponse>(data);
                CreateWorldPoints(response.results);
                CreateRoutePoints();
            }
            else
            {
                StartCoroutine(SearchForGooglePlaces());
            }

            StartCoroutine(StartLocationServices());
        }

        #endregion

        #region Editor methods

        private void Reset()
        {
            apiKey = "AIzaSyBhKgyaouRTotZT_yFgNiZ1GhgJbylUVv0";

            useExternalGps = false;

            searchRadius = 50;
      
            deviceLocation = new Location(51.1132478431398, 17.0187119638331);
            targetLocation = new Location(51.1110261277860, 17.0213091373444);

            enableDebug = true;
            dataFileName = "data.json";
        }

        private void OnValidate()
        {
            //clamping distances and time
            gpsAccuracy = Mathf.Max(gpsAccuracy, 0.1f);
            searchRadius = Mathf.Max(searchRadius, 1.0f);
            deviceRadius = Mathf.Clamp(deviceRadius, 0.5f, searchRadius);
            updateDistance = Mathf.Max(updateDistance, 0.1f);
            updateInterval = Mathf.Max(updateInterval, 1.0f);

            //calcalute device coor in x and y
            calculatedDevicePoint = Scholar.GPSCoorToPoint(deviceLocation);
            calculatedTargetPoint = Scholar.GPSCoorToPoint(targetLocation);
            calculatedStartPoint = Scholar.GPSCoorToPoint(startLocation);
        }

        #endregion

        #region Loop methods

        private void Update()
        { }

        #endregion

        #region Private methods

        private IEnumerator StartLocationServices()
        {
            StartLocation = DeviceLocation;

            if (useExternalGps)
            {
                externalReceiver.OnDataReceived.AddListener((data) =>
                {
                    DeviceLocation = JsonUtility.FromJson<Location>(data);
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
                        DeviceLocation = new Location(inputHandler.LocationService.lastData);
                        //DeviceLocation = new Location(inputHandler.LocationService.lastData.latitude, 
                        //    inputHandler.LocationService.lastData.longitude);
                        yield return interval;
                    }
                }
            }
        }

        private IEnumerator SearchForGoogleRoutes()
        {
            var apiRequest = new UnityWebRequest(SearchForRouteUrlFormat)
            {
                downloadHandler = new DownloadHandlerBuffer()
            };
            yield return apiRequest.SendWebRequest();

            if (apiRequest.error != null)
            {
                Debug.LogError(apiRequest.error);
                yield break;
            }

            var data = apiRequest.downloadHandler.text;
            SaveDebugData(data);
        }

        private IEnumerator SearchForGooglePlaces()
        {
            var apiRequest = new UnityWebRequest(SearchInRadiusUrlFormat)
            {
                downloadHandler = new DownloadHandlerBuffer()
            };
            yield return apiRequest.SendWebRequest();

            if (apiRequest.error != null)
            {
                Debug.LogError(apiRequest.error);
                yield break;
            }

            var data = apiRequest.downloadHandler.text;
            SaveDebugData(data);
        }

        private void SaveDebugData(string data)
        {
            File.WriteAllText(Application.persistentDataPath + "/" + dataFileName, data);
        }

        #endregion

        #region Public methods

        public void SetApiKey(string newKey)
        {
            apiKey = newKey;
        }

        public void AddExcludedType(string type)
        {
            typesToExclude.Add(type);
        }

        public void RemoveExcludedType(string type)
        {
            typesToExclude.Remove(type);
        }

        public void AddPredefinedPlace(GooglePlace place)
        {
            predefinedPlaces.Add(place);
        }

        public void RemovePredefinedPlace(GooglePlace place)
        {
            predefinedPlaces.Remove(place);
        }

        public void CreateWorldPoints(GooglePlace[] places)
        {
            knownTargets.Clear();
     
            try
            {
                var allPlaces = places.Concat(predefinedPlaces);

                foreach (var place in allPlaces)
                {
                    //excluding desired types
                    var typesToInclude = typesToExclude.Except(place.types).ToArray();
                    if (!typesToInclude.SequenceEqual(typesToExclude)) continue;
                    //calculating world position
                    var position = calculatedDevicePoint - Scholar.GPSCoorToPoint(place.geometry.location);
                    if (position.magnitude < deviceRadius) continue;
                    var target = Instantiate(worldTargetPrefab);
                    //setting new target
                    target.transform.SetParent(worldRoot);
                    target.transform.position = new Vector3(position.x, 0, position.y);

                    target.Place = new PointOfInterest
                    {
                        name = place.name,
                        address = new Address
                        {
                            location = place.geometry.location
                        }
                    };

                    //saving new target
                    knownTargets.Add(target);
                }
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e);
            }
        }

        public void UpdateWorldPoints()
        {
            knownTargets.ForEach(target =>
            {
                var position = calculatedDevicePoint - Scholar.GPSCoorToPoint(target.Location);
                target.transform.localPosition = new Vector3(position.x, 0, position.y);
            });
        }

        public void CreateRoutePoints()
        {
            var startPoint = calculatedDevicePoint - Scholar.GPSCoorToPoint(startLocation);
            var targetPoint = calculatedDevicePoint - Scholar.GPSCoorToPoint(targetLocation);
            var stepPoints = new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(targetPoint.x, 0, targetPoint.y)
            };

            try
            {
                worldRouteRenderer.positionCount = stepPoints.Count;
                worldRouteRenderer.SetPositions(stepPoints.ToArray());
            }
            catch (UnityException)
            {
                ThreadManager.ExecuteInUpdate(() =>
                {
                    worldRouteRenderer.positionCount = stepPoints.Count;
                    worldRouteRenderer.SetPositions(stepPoints.ToArray());
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void CreateRoutePoints(GoogleRoute[] routes)
        {
            if (routes == null) return;

            var stepPoints = new List<Vector3>();
            var pointPosition = new Vector3();

            foreach (var route in routes)
            {
                foreach (var leg in route.legs)
                {
                    pointPosition = calculatedDevicePoint - Scholar.GPSCoorToPoint(leg.start_location);
                    stepPoints.Add(new Vector3(pointPosition.x, 0, pointPosition.y));

                    foreach (var step in leg.steps)
                    {
                        pointPosition = calculatedDevicePoint - Scholar.GPSCoorToPoint(step.end_location);
                        stepPoints.Add(new Vector3(pointPosition.x, 0, pointPosition.y));
                    }
                }
            }

            try
            {
                worldRouteRenderer.positionCount = stepPoints.Count;
                worldRouteRenderer.SetPositions(stepPoints.ToArray());
            }
            catch (UnityException)
            {
                ThreadManager.ExecuteInUpdate(() =>
                {
                    worldRouteRenderer.positionCount = stepPoints.Count;
                    worldRouteRenderer.SetPositions(stepPoints.ToArray());
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            lastUsedRoutes.Clear();
            lastUsedRoutes.AddRange(routes);
        }

        public void UpdateRoutePoints()
        {
            if (lastUsedRoutes.Count == 0)
            {
                CreateRoutePoints();
                return;
            }
            CreateRoutePoints(lastUsedRoutes.ToArray());
        }

        #endregion

        #region Properties

        private string SearchForRouteUrlFormat
        {
            get { return string.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0}&destination={1}&mode={2}&key={3}", deviceLocation, targetLocation, travelMode.ToString(), apiKey); }
        }

        private string SearchInRadiusUrlFormat
        {
            get { return string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0}&radius={1}&key={2}", deviceLocation, searchRadius, apiKey); }
        }

        public bool SearchRoutes
        {
            get { return searchRoutes; }
            set { searchRoutes = value; }
        }

        public bool SearchBuildings
        {
            get { return searchPlaces; }
            set { searchPlaces = value; }
        }

        public float SearchRadius
        {
            get { return searchRadius; }
            set { searchRadius = value; }
        }

        public Location StartLocation
        {
            get { return startLocation; }
            private set
            {
                startLocation = value;
                calculatedStartPoint = Scholar.GPSCoorToPoint(value);
            }
        }

        public Location DeviceLocation
        {
            get { return deviceLocation; }
            private set
            {
                deviceLocation = value;
                calculatedDevicePoint = Scholar.GPSCoorToPoint(value);
                UpdateRoutePoints();
                UpdateWorldPoints();
            }
        }

        public Location TargetLocation
        {
            get { return targetLocation; }
            set
            {
                targetLocation = value;
                calculatedTargetPoint = Scholar.GPSCoorToPoint(value);
                //TODO: route update
            }
        }

        public GoogleTravelMode TravelMode
        {
            get { return travelMode; }
            set { travelMode = value; }
        }

        public Transform WorldRoot
        {
            get { return worldRoot; }
            set
            {
                worldRoot = value;
                knownTargets.ForEach(item => item.transform.parent = worldRoot);
                worldRouteRenderer.transform.parent = worldRoot;
            }
        }

        public PlaceRenderer WorldTargetPrefab
        {
            get { return worldTargetPrefab; }
            set { worldTargetPrefab = value; }
        }

        public LineRenderer RouteRenderer
        {
            get { return worldRouteRenderer; }
            set { worldRouteRenderer = value; }
        }

        public Network.NetworkServer ExternalReceiver
        {
            get { return externalReceiver; }
            set { externalReceiver = value; }
        }

        #endregion

        public enum GoogleTravelMode
        {
            walking,
            driving,
            bicycling,
            transit
        }

        [Serializable]
        public class GooglePlace
        {
            [Serializable]
            public struct Geometry
            {
                public Location location;
            }

            public string name;
            public string icon;
            public string id;

            public Geometry geometry;

            public string[] types;
        }

        [Serializable]
        public class GoogleRoute
        {
            [Serializable]
            public struct Bounds
            {
                public Location northeast;
                public Location southwest;
            }

            [Serializable]
            public struct Leg
            {
                public string end_address;
                public string start_address;

                public Location end_location;
                public Location start_location;

                public Step[] steps;
            }

            [Serializable]
            public struct Step
            {
                public Location end_location;
                public Location start_location;
            }

            public Bounds bounds;       
            public string copyrights;

            public Leg[] legs;
        }

        [Serializable]
        public class GooglePlacesApiResponse
        {
            public GooglePlace[] results;
        }

        [Serializable]
        public class GoogleDirectionsApiResponse
        {
            public string status;

            public GoogleRoute[] routes;
        }
    }
}