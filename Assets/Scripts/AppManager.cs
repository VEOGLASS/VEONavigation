using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//TODO: clearing world places;

namespace XploriaAR
{
    [DisallowMultipleComponent, RequireComponent(typeof(LocationManager))]
    public class AppManager : MonoBehaviour
    {
        private readonly List<PlaceRenderer> knownPlaces = new List<PlaceRenderer>();

        private LocationManager locationManager;
        private ExternalDataManager navigationManager;

        #region Inspector fields

        [Separator, Header("World Creation")]

        [SerializeField]
        private WorldManager worldRoot;
        [SerializeField]
        private RouteRenderer worldRouteRenderer;
        [SerializeField]
        private PlaceRenderer worldPlaceRendererPrefab;

        [Space]

        [SerializeField]
        private int maxClosePlaces;

        [SerializeField]
        private List<PlaceDisplayMode> displayModes = new List<PlaceDisplayMode>();

        [Separator, Header("World Data")]

        [SerializeField]
        private Location deviceLocation;

        [Space]

        [SerializeField]
        private Trip trip;

        [OrderedSpace]

        [SerializeField, ReorderableList(ListStyle.Lined, "POI")]
        private List<PointOfInterest> predefinedPlaces = new List<PointOfInterest>();

        #endregion

        #region Init and deinit methods

        /// <summary>
        /// Manager injection.
        /// </summary>
        private void Awake()
        {
            /* 
             * Self-Injection 
             */
            locationManager = GetComponent<LocationManager>();
            navigationManager = GetComponent<ExternalDataManager>();

            //creation of all objects
            CreateWorldPlaces(null);
            CreateRoutePoints();
            CreateTrip(trip);
        }

        /// <summary>
        /// Manager initialization.
        /// </summary>
        private void Start()
        {
            //subscribing location event
            locationManager?.AddOnLocationChangeListener(OnLocationUpdate);

            //mocking first GPS location
            OnLocationUpdate(deviceLocation);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Inspector fields validation.
        /// </summary>
        private void OnValidate()
        {
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            OnLocationUpdate(deviceLocation);

            displayModes.Sort((a, b) => a.distance.CompareTo(b.distance));
            displayModes.Reverse();

            transform.hideFlags = HideFlags.HideInInspector;
        }
#endif

        #endregion

        #region Private methods

        private void OnNavDataUpdate(string data)
        {
            throw new NotImplementedException();
        }

        private void OnLocationUpdate(Location location)
        {
            DeviceLocation = location;

            UpdateWorldPlaces();
            UpdateRoutePoints();
        }

        private Vector2 GetRelativePoint(Location location)
        {
            return new Vector3(0, 0, 0);
        }

        private Vector3 GetRelativePosition(Location location)
        {
            var point = GetRelativePoint(location);
            return new Vector3(point.x, 0, point.y);
        }

        private Vector3 GetRelativePosition(Location deviceLocation, Location targetLocation)
        {
            throw new NotImplementedException();
        }

        private void SetPlaceSettingsAt(int index)
        {
            var place = knownPlaces[index];
            //restrict number of normal-way displayed places
            if (index < maxClosePlaces)
            {
                //update place settings depending on distance
                for (var i = 0; i < displayModes.Count; i++)
                {
                    if (displayModes[i].distance > place.Distance && i != displayModes.Count - 1)
                    {
                        continue;
                    }
                    place.SetSettings(displayModes[i].settings);
                    break;
                }
            }
            else
            {
                place.SetSettings(displayModes.First().settings);
            }
        }

        #endregion

        #region Public methods

        public void CreateWorldPlace(PointOfInterest place)
        {
            try
            {
                var placeRenderer = Instantiate(worldPlaceRendererPrefab);
                var position = GetRelativePosition(place.address.location);
                //setting new target            
                placeRenderer.transform.SetParent(worldRoot.transform);
                placeRenderer.transform.localPosition = position;
                placeRenderer.Place = place;
                //saving new target
                knownPlaces.Add(placeRenderer);
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e);
            }
        }

        public void CreateWorldPlaces(PointOfInterest[] places, bool clearAll = false)
        {
            //TODO: clear(destroy) all previously created places
            knownPlaces.Clear();

            try
            {
                var allPlaces = places != null
                    ? predefinedPlaces.Concat(places)
                    : predefinedPlaces;
                foreach (var place in allPlaces) CreateWorldPlace(place);
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e);
            }
        }

        public void ClearWorldPlaces(PointOfInterest[] placesToExclude = null)
        {
            throw new NotImplementedException();
        }

        public void UpdateWorldPlaces()
        {
            knownPlaces.ForEach(place =>
            {
                //distance + bearing calculation 
                var distance = Scholar.Distance(deviceLocation, place.Location, 'm');
                var bearing = Scholar.BearingTo(deviceLocation, place.Location);
                place.Distance = (float)distance;
                place.Bearing = (float)bearing;
                // TODO: 179 ?
                var bearingRadians = Scholar.Deg2Rad(bearing) + 179;

                // Rotate POI's
                var xRot = -Math.Sin(bearingRadians) * (distance - 0) + 0;
                var zRot = +Math.Cos(bearingRadians) * (distance - 0) + 0;
                place.transform.localPosition = new Vector3((float)-xRot, 0, (float)zRot);
            });
            knownPlaces.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            knownPlaces.ForEachWithIndex((place, index) =>
            {
                SetPlaceSettingsAt(index);
            });
        }

        public void CreateRoutePoints()
        {
            var distance = Scholar.Distance(deviceLocation, TargetLocation, 'm');
            var bearing = Scholar.BearingTo(deviceLocation, TargetLocation);
            // TODO: 179 ?
            var bearingRadians = Scholar.Deg2Rad(bearing) + 179;

            // Rotate POI's
            var xRot = -Math.Sin(bearingRadians) * (distance - 0) + 0;
            var zRot = +Math.Cos(bearingRadians) * (distance - 0) + 0;
            worldRouteRenderer.SetRoute(new Vector3(0, 0, 0), new Vector3((float)-xRot, 0, (float)zRot));
        }

        public void UpdateRoutePoints()
        {
            CreateRoutePoints();
        }

        public void CreateTrip(params Waypoint[] waypoints)
        {
            CreateTrip(new Trip { waypoints = waypoints });
        }

        public void CreateTrip(Trip trip)
        {
            this.trip = trip;
            UpdateRoutePoints();
        }

        public void ClearTrip()
        {
            throw new NotImplementedException();
        }

        public void AddPredefinedPlace(PointOfInterest place)
        {
            predefinedPlaces.Add(place);
        }

        public void RemovePredefinedPlace(PointOfInterest place)
        {
            predefinedPlaces.Remove(place);
        }

        public void AddDisplayMode(PlaceDisplayMode mode)
        {
            if (displayModes == null) displayModes = new List<PlaceDisplayMode>();
            displayModes.Add(mode);
            displayModes.Sort((a, b) => a.distance.CompareTo(b.distance));
            displayModes.Reverse();
        }

        public void RemoveDisplayMode(PlaceDisplayMode mode)
        {
            displayModes.Remove(mode);
            displayModes.Sort((a, b) => a.distance.CompareTo(b.distance));
            displayModes.Reverse();
        }

        public void DisplayPlaces(bool value)
        {
            if (DisplayingPlaces == value) return;

            //TODO: proper method implementation(per behaviour);
            knownPlaces.ForEach(place => place.gameObject.SetActive(value));
            worldRouteRenderer.gameObject.SetActive(value);
            DisplayingPlaces = value;
        }

        public void ForceWaypointToPlace(string placeName)
        {
            ForceWaypointToPlace(knownPlaces.Find(poi => poi.Place.name == placeName).Place);
        }

        public void ForceWaypointToPlace(PointOfInterest place)
        {
            CreateTrip(new Waypoint() { location = place.address.location });
        }

        #endregion


        public bool DisplayingPlaces { get; private set; } = true;

        public int MaxClosePlaces
        {
            get => maxClosePlaces;
            set => maxClosePlaces = value;
        }

        public WorldManager WorldRoot
        {
            get => worldRoot;
            set
            {
                worldRoot = value;
                worldRouteRenderer.transform.parent = worldRoot.transform;      //setting new parent for line renderer
                knownPlaces.ForEach(item =>
                {
                    item.transform.parent = worldRoot.transform;                //setting new parent for each place(POI)
                });
            }
        }

        public RouteRenderer RouteRenderer
        {
            get => worldRouteRenderer;
            set => worldRouteRenderer = value;
        }

        public PlaceRenderer PlaceRendererPrefab
        {
            get => worldPlaceRendererPrefab;
            set => worldPlaceRendererPrefab = value;
        }

        /// <summary>
        /// Current known GPS device location.
        /// </summary>
        public Location DeviceLocation
        {
            get => deviceLocation;
            private set
            {
                deviceLocation = value;
            }
        }

        /// <summary>
        /// Current GPS target location. If there is no valid target - return value will be
        /// the same as the location of the device.
        /// </summary>
        public Location TargetLocation
        {
            get => trip.waypoints != null && trip.waypoints.Length > 0 ? trip.waypoints[0].location : deviceLocation;
        }
    }
}