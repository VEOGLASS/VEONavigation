using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace XploriaAR
{
    [DisallowMultipleComponent]
    public sealed class WorldManager : MonoBehaviour
    {
        #region Const fields

        private const int desiredFramesRatio = 60;

        #endregion

        #region Private fields

        /// <summary>
        /// All available inputs interface.
        /// </summary>
        private IHandleInput inputHandler;

        /// <summary>
        /// Rotation rate drift per frame.
        /// </summary>
        private Vector3 driftOffset;

        /// <summary>
        /// Last readed rotation rate.
        /// </summary>
        private Vector3 rotationRate;

        /// <summary>
        /// Last readed raw accelerationrate;
        /// </summary>
        private Vector3 acceleration;

        /// <summary>
        /// Current attitude. Readed in loop method.
        /// </summary>
        private Quaternion attitude;

        /// <summary>
        /// List of tracked headings.
        /// </summary>
        private List<float> headings = new List<float>();
    
        #endregion

        #region Inspector fields

        [SerializeField, ConditionalField("useCameraRotation")]
        private Camera arCamera;

        [Space]

        [SerializeField]
        private Quaternion basicRotation = Quaternion.Euler(0, 0, 0);
        [SerializeField]
        private Quaternion northRotation = Quaternion.Euler(0, 180, 0);

        [Header("Measurement"), Separator]

        [SerializeField, ReadOnly]
        private float lastHeading;
        [HideInInspector]
        private float readedHeading;
        [SerializeField]
        private float smoothSpeed = 2.0f;

        [SerializeField, Tooltip("Number of last tracked headings.")]
        private int headCapacity = 10;

        [Space]

        [SerializeField, Tooltip("If true - application will use gyroscope values to manipulate desired Camera's rotation.")]
        private bool useCameraRotation;
        [SerializeField, ConditionalField("useCameraRotation")]
        private bool useWorldAttitude = true;
        [SerializeField]
        private bool useAcceleration = true;
        [SerializeField]
        private bool useTrueHeading = true;

        #endregion
 
        #region Init methods

        /// <summary>
        /// Injection and first initialization.
        /// </summary>
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

        /// <summary>
        /// Basic initialization.
        /// </summary>
        private void Start()
        {
            StartCoroutine(InitializeDataSensors(true, true));
            StartCoroutine(InitializeWorldTransform());
        }
        
        /// <summary>
        /// Inspector fields reset.
        /// </summary>
        private void Reset()
        {
            smoothSpeed = 2.0f;

            headCapacity = 10;

            basicRotation = Quaternion.Euler(0, 0, 0);
            northRotation = Quaternion.Euler(0, 180, 0);

            useCameraRotation = false;
            useWorldAttitude = true;
            useAcceleration = true;
            useTrueHeading = true;
        }

        /// <summary>
        /// Inspector fields validation.
        /// </summary>
        private void OnValidate()
        {
            if (!arCamera) arCamera = Camera.main;

            smoothSpeed = Mathf.Max(1.0f, smoothSpeed);
            headCapacity = Mathf.Max(1, headCapacity);

            UseCameraRotation = useCameraRotation;
            UseWorldAttitude = useWorldAttitude;      
            UseAcceleration = useAcceleration;
            UseTrueHeading = useTrueHeading;
        }

        #endregion

        #region Loop methods

        private void Update()
        {
            #region Compass measurement

            //compass measurement
            //----------------------------------------------------
            readedHeading = useTrueHeading ? inputHandler.Compass.trueHeading : inputHandler.Compass.magneticHeading;
            AddHeading(readedHeading);
            
            //rotation using only compass(OY axis)
            //----------------------------------------------------
            //avarage heading
            //WorldRotation = Quaternion.Euler(0, -GetHeading(), 0); 
            //only last value
            //WorldRotation = Quaternion.Euler(0, -lastHeading, 0);
            //linear approximation
            //WorldRotation = Quaternion.Lerp(WorldRotation, Quaternion.Euler(0, -readedHeading, 0), Time.deltaTime * smoothSpeed);
            
            lastHeading = readedHeading;

            #endregion

            #region Gyroscope measurement

            //full gyroscope-based rotation(OX, OY, OZ axis)
            //----------------------------------------------------
            //reading device to world attitude
            attitude = inputHandler.Gyroscope.attitude;
            //reading unbiased rotation change
            rotationRate = inputHandler.Gyroscope.rotationRateUnbiased * desiredFramesRatio / (1.0f / Time.unscaledDeltaTime) + DriftOffset;
            rotationRate.x = Mathf.Abs(rotationRate.x) > 0.01f ? rotationRate.x : 0.0f;
            rotationRate.y = Mathf.Abs(rotationRate.y) > 0.01f ? rotationRate.y : 0.0f;
            rotationRate.z = Mathf.Abs(rotationRate.z) > 0.01f ? rotationRate.z : 0.0f;

            if (useCameraRotation)
            {
                if (useWorldAttitude)
                {
                    //setting corrections        
                    var selfRotation = new Vector3(0, 0, 180);
                    var worldRotation = new Vector3(90, 180, 0);
                    var prevRotation = CameraRotation;
                    //setting new rotation
                    CameraRotation = attitude;
                    arCamera.transform.Rotate(selfRotation, Space.Self);
                    arCamera.transform.Rotate(worldRotation, Space.World);
                    //lerping quaternion for smooth change
                    CameraRotation = Quaternion.Lerp(prevRotation, CameraRotation, Time.deltaTime * smoothSpeed);
                }
                else
                {
                    //creating rotation change
                    arCamera.transform.eulerAngles += new Vector3(-rotationRate.x, -rotationRate.y, 0);
                }
            }
            else
            {
                //in world rotation use only compass 
                WorldRotation = Quaternion.Lerp(WorldRotation, Quaternion.Euler(0, -readedHeading, 0), Time.deltaTime * smoothSpeed);
            }

            #endregion

            #region Accelerometer measurement

            if (!useAcceleration) return;

            //TODO:
            //acceleration measurement
            //----------------------------------------------------
            acceleration = inputHandler.Acceleration - inputHandler.Gyroscope.gravity;
            WorldPosition -= new Vector3(acceleration.x, 0, acceleration.z);

            #endregion
        }

        #endregion

        #region Private methods

        private IEnumerator InitializeWorldTransform()
        {
            yield return null;

            WorldRotation = useCameraRotation ? northRotation : basicRotation;      
        }

        private IEnumerator InitializeDataSensors(bool useGyro, bool useCompass)
        {
            //setting gyro if needed
            inputHandler.Gyroscope.enabled = useGyro;
            inputHandler.Gyroscope.updateInterval = 1 / desiredFramesRatio;

            if (!useCompass) yield break;

            //setting compass if needed
            while (!inputHandler.Compass.enabled)
            {
                inputHandler.Compass.enabled = true;
                yield return null;
            }
        }

        private float GetHeading()
        {
            if (useWorldAttitude)
            {
                return CameraRotation.eulerAngles.y;
            }
            var sum = 0.0f;
            foreach (var heading in headings) sum += heading;
            return sum / headings.Count;
        }

        private void AddHeading(float heading)
        {
            headings.Add(heading);
            if (headings.Count > headCapacity) headings.RemoveAt(0);            
        }

        #endregion

        #region Properties

        public bool UseCameraRotation
        {
            get { return useCameraRotation; }
            set
            {
                useCameraRotation = value;
                WorldRotation = useCameraRotation ? northRotation : basicRotation;
            }
        }

        public bool UseWorldAttitude
        {
            get { return useWorldAttitude; }
            set { useWorldAttitude = value; }
        }

        public bool UseAcceleration
        {
            get { return useAcceleration; }
            set { useAcceleration = value; }
        }

        public bool UseTrueHeading
        {
            get { return useTrueHeading; }
            set { useTrueHeading = value; }
        }

        public float SmoothSpeed
        {
            get { return smoothSpeed; }
            set { smoothSpeed = Mathf.Max(1.0f, value); }
        }

        public float LastHeading
        {
            get { return GetHeading(); }
        }

        public Vector3 DriftOffset
        {
            get { return new Vector3(-0.02f, -0.02f, 0); }
        }

        public Vector3 WorldPosition
        {
            get { return transform.position; }
            private set { transform.position = value; }
        }

        public Quaternion WorldRotation
        {
            get { return transform.rotation; }
            private set { transform.rotation = value; }
        }

        public Quaternion CameraRotation
        {
            get { return useCameraRotation ? arCamera.transform.rotation : new Quaternion(); }
            set
            {
                if (!useCameraRotation) return;
                arCamera.transform.rotation = value;
            }
        }

        #endregion
    }
}