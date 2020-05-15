using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace XploriaAR
{
    using XploriaAR.UI;

    public class DisplayManager : MonoBehaviour
    {
        [SerializeField, Range(0.0001f, 10.0f)]
        private float updateInterval = 0.01f;

        [Header("Sources"), Separator]

        [SerializeField]
        private WorldManager worldManager;
        [SerializeField]
        private LocationManager locationManager;
        [SerializeField]
        private ExternalDataManager navigationManager;

        [Header("Display Objects"), Separator]

        [SerializeField]
        private WindManager windManager;
        [SerializeField]
        private CompassManager compassManager;

        [Header("Display Data"), Separator]

        [SerializeField]
        private UiNmeaDataText speedText;
        [SerializeField]
        private UiNmeaDataText deepnessText;
        [SerializeField]
        private UiNmeaDataText windStrengthText;

        [Header("Display Debug"), Separator]

        public bool DebugAllowed = false;

        [SerializeField, ConditionalField("DebugAllowed")]
        private TextMeshProUGUI fpsText;
        [SerializeField, ConditionalField("DebugAllowed")]
        private TextMeshProUGUI gpsCoorText;
        [SerializeField, ConditionalField("DebugAllowed")]
        private TextMeshProUGUI headingText;
        [SerializeField, ConditionalField("DebugAllowed")]
        private TextMeshProUGUI rotationText;

        private void Start()
        {
            if (!navigationManager) return;

            navigationManager.AddOnDataChangeListener(OnDataUpdate);
        }

        private void OnEnable()
        {
            StartCoroutine(DisplayLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        //tmp
        private void Update()
        {
            if (windManager.GetDirection() > 359) windManager.SetDirection(0);
            windManager.SetDirection(windManager.GetDirection() + 40 * Time.deltaTime);
        }

        private void OnDataUpdate(NavigationApiResponse response)
        {
            speedText?.SetValue(response.data.speed);
            deepnessText?.SetValue(0);
            windStrengthText?.SetValue(response.data.windStrength);

            CompassManager?.DisplayCurrentCurse(response.data.currentCourse);
            CompassManager?.DisplayDesiredCurse(response.data.desiredCourse);

            WindManager?.SetDirection(response.data.windDirection);
        }

        /// <summary>
        /// Main display loop.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DisplayLoop()
        {
            var interval = new WaitForSecondsRealtime(updateInterval);
            while(true)
            {
                yield return interval;
                if (!DebugAllowed) continue;

                fpsText?.SetText(((int)(1.0f / Time.unscaledDeltaTime)).ToString());
                gpsCoorText?.SetText("Lat:" + locationManager.DeviceLocation.lat + "\nLng:" + locationManager.DeviceLocation.lng);
                headingText?.SetText("Heading:" + worldManager.LastHeading);
                rotationText?.SetText("Rotation:" + worldManager.CameraRotation.eulerAngles);
            }
        }


        public float UpdateInterval
        {
            get { return updateInterval; }
            set { updateInterval = Mathf.Clamp(value, 0.0001f, 1.0f); }
        }


        public WorldManager WorldManager
        {
            get { return worldManager; }
            set { worldManager = value; }
        }

        public LocationManager LocationManager
        {
            get { return locationManager; }
            set { locationManager = value; }
        }

        public ExternalDataManager NavigationManager
        {
            get { return navigationManager; }
            set { navigationManager = value; }
        }


        public WindManager WindManager
        {
            get => windManager;
            set => windManager = value;
        }

        public CompassManager CompassManager
        {
            get => compassManager;
            set => compassManager = value;
        }


        public UiNmeaDataText SpeedText
        {
            get => speedText;
            set => speedText = value;
        }

        public UiNmeaDataText DeepnessText
        {
            get => deepnessText;
            set => deepnessText = value;
        }

        public UiNmeaDataText WindStrengthText
        {
            get => windStrengthText;
            set => windStrengthText = value;
        }


        public TextMeshProUGUI FpsText
        {
            get { return fpsText; }
            set { fpsText = value; }
        }

        public TextMeshProUGUI GpsCoorText
        {
            get { return gpsCoorText; }
            set { gpsCoorText = value; }
        }

        public TextMeshProUGUI HeadingText
        {
            get { return headingText; }
            set { headingText = value; }
        }

        public TextMeshProUGUI RotationText
        {
            get { return rotationText; }
            set { rotationText = value; }
        }
    }
}