using System;

using UnityEngine;
using UnityEngine.UI;

//TODO: dynamic icons; height fix;

namespace XploriaAR
{
    using XploriaAR.UI;

    [Serializable]
    public struct PlaceDisplayMode
    {
        public string name;
        [Tooltip("Desired use distance.")]
        public float distance;

        public PlaceSettings settings;
    }

    /// <summary>
    /// Known as POI renderer. Specific place in world display behaviour.
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent, SelectionBase]
    public class PlaceRenderer : MonoBehaviour
    {
        /// <summary>
        /// Desired camera object.
        /// </summary>
        private Camera arCamera;
        /// <summary>
        /// Associated gameObject's <see cref="Animator"/> component.
        /// </summary>
        private Animator animator;
        /// <summary>
        /// Associated gameObject's <see cref="CanvasGroup"/> component.
        /// </summary>
        private CanvasGroup canvasGroup;

        /// <summary>
        /// Variable used in raycasting.
        /// </summary>
        private RaycastHit hit;

        #region Inspector fields

        [SerializeField]
        private float bearing;
        [SerializeField]
        private float distance;

        [Separator]

        [SerializeField]
        private PointOfInterest place;

        [Space]

        [Space(2), Header("Display"), Separator]

        [SerializeField, ReadOnly]
        private PlaceSettings settings;

        [SerializeField]
        private DisplayComponents displayComponents;

        #endregion

        private void Awake()
        {
            animator = GetComponent<Animator>();
            canvasGroup = GetComponent<CanvasGroup>();

            //TODO: this component should be injected
            arCamera = Camera.main;
        }

        private void Start()
        { }

        private void Reset()
        { }

        private void Update()
        {
            CheckRotation();
            CheckHeight();
            CheckScale();

            UpdateDisplay();
        }

        [Obsolete]
        private void CheckVisibility()
        {
            //TODO:
            var cameraDir = (arCamera.transform.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, cameraDir, out hit/*,cameraDis , gameObject.layer*/))
            {

            }
            /*
            if (useAngleDistance)
            {
                //checking visibility using angle between camera's look direction and target's direction
                var targetDir = transform.position - arCamera.transform.position;
                IsVisible = Vector3.Angle(targetDir, arCamera.transform.forward) < visibleRadius;
            }
            else
            {
                //checking visibility using distance between camera's look direction and target's position
                var plane = new Plane(arCamera.transform.position, arCamera.transform.forward, arCamera.transform.up);
                IsVisible = Mathf.Abs(plane.GetDistanceToPoint(transform.position)) < visibleDistance;
            }
            */
        }

        //[Obsolete("Use Billboard shader instead.")]
        //NOTE: Cannot use Billboard shaders inside single Canvas because of batching.
        private void CheckRotation()
        {
            transform.LookAt(arCamera.transform);
        }

        private void CheckHeight()
        {
            if (!settings) return;
            //setting correct height for each child
            for (var i = 0; i < transform.childCount; i++)
            {
                var position = transform.GetChild(i).transform.position;
                //setting desired height 
                position.y = settings.useFixedHeight 
                    ? Distance * settings.fixedHeight * arCamera.fieldOfView 
                    : 0;
                transform.GetChild(i).transform.position = position;
            }
        }

        private void CheckScale()
        {
            if (!settings) return;
            //setting correct scale for each child
            for (var i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).localScale = settings.useFixedScale 
                    ? Vector3.one * Distance * settings.fixedSize * arCamera.fieldOfView 
                    : Vector3.one;
            }
        }

        private void UpdateDisplay()
        {
            //TODO:icons
            //displayComponents.DisplayIcon();
            displayComponents.DisplayPlace(Place);   
            displayComponents.DisplayDistance(Bearing, Distance);
        }

        /// <summary>
        /// Set display settings.
        /// </summary>
        /// <param name="newSettings"></param>
        public void SetSettings(PlaceSettings newSettings)
        {
            if (!newSettings)
            {
                return;
            }
            settings = newSettings;

            //setting visibility of poiting line and type icon image
            displayComponents.typeImage.enabled = settings.useTypeIcon;
            displayComponents.linePointer.enabled = settings.usePointingLine;

            //setting correct colors to label background and type icon image
            displayComponents.typeImage.color = settings.iconColor;
            displayComponents.labelImage.color = settings.labelColor;
            displayComponents.outlineImage.color = settings.outlineColor;

            //setting proper font sizes to all texts 
            displayComponents.typeText.fontSize = settings.typeTextFontSize;
            displayComponents.distText.fontSize = settings.distTextFontSize;
            displayComponents.nameText.fontSize = settings.nameTextFontSize;
            //setting maximum posible characters
            displayComponents.nameText.maxVisibleCharacters = settings.maxNameCharacters;

            displayComponents.typeText.alignment = settings.typeTextOption;
            displayComponents.distText.alignment = settings.distTextOption;
            displayComponents.nameText.alignment = settings.nameTextOption;

            //setting proper icon type image position and size
            var iconTransform = displayComponents.typeImage.rectTransform;
            iconTransform.sizeDelta = settings.iconSize;
            iconTransform.anchorMin = settings.iconAnchorMin;
            iconTransform.anchorMax = settings.iconAnchorMax;

            if (settings.usePointingLine) return;
            //setting proper label position
            var labelRect = displayComponents.labelImage.rectTransform;
            labelRect.anchoredPosition = new Vector2(0, 0);
        }

        /// <summary>
        /// Forces a behaviour to update.
        /// </summary>
        public void SetDirty()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Current distance to user.
        /// </summary>
        public float Distance
        {
            get => distance;
            set => distance = value;
        }

        /// <summary>
        /// Calculated place to north bearing.
        /// </summary>
        public float Bearing
        {
            get => bearing;
            set => bearing = value;
        }

        /// <summary>
        /// Current world space position of place label image.
        /// </summary>
        public Vector3 LabelPosition
        {
            get => displayComponents.labelImage.transform.position;
            set => displayComponents.labelImage.transform.position = value;
        }

        /// <summary>
        /// Current place location.
        /// </summary>
        public Location Location
        {
            get { return place.address.location; }
        }

        /// <summary>
        /// Associated place.
        /// </summary>
        public PointOfInterest Place
        {
            get { return place; }
            set
            {
                place = value;
                name = place.name;
            }
        }

        /// <summary>
        /// Provides all available display components for <see cref="PlaceRenderer"/> behaviour.
        /// </summary>
        [Serializable]
        public struct DisplayComponents
        {
            private const string cuttedTextAddition = "...";

            public Image typeImage;
            public Image labelImage;           
            public Image outlineImage;

            [Space]

            public TMPro.TextMeshProUGUI nameText;
            public TMPro.TextMeshProUGUI typeText;
            public TMPro.TextMeshProUGUI distText;

            [Space]

            public UiWorldLabel linePointer;

            public void DisplayPlace(PointOfInterest place)
            {
                var name = place.name;       
                if (name.Length > nameText.maxVisibleCharacters - cuttedTextAddition.Length)
                {
                    name = name.Remove(Mathf.Max(nameText.maxVisibleCharacters - cuttedTextAddition.Length, 0));
                    name += cuttedTextAddition;
                }

                nameText.text = name;
                var size = new Vector2(nameText.preferredWidth, 1.25f);
                nameText.rectTransform.sizeDelta = size;
                typeText.text = place.type;
            }

            public void DisplayIcon(Sprite icon)
            {
                typeImage.sprite = icon;
            }

            public void DisplayDistance(float bearing, float distance)
            {
                distText.text = (int)distance + "m";
            }
        }
    }
}