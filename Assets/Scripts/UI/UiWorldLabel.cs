using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Random = UnityEngine.Random;

namespace XploriaAR.UI
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform), typeof(BoxCollider))]
    public class UiWorldLabel : MonoBehaviour
    {
        private RectTransform rectTransform;

        [SerializeField]
        private UILineRenderer lineRenderer;

        [Space]

        [SerializeField]
        private float regroupingSpeed = 2.0f;

        [Space]

        [SerializeField]
        private Vector2 endPoint;
        [SerializeField]
        private Vector2 offsetVector;
        private Vector2 lastPosition;

        private Vector2[] points;


        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            UpdateLine();
        }

        private void OnEnable()
        {
            lineRenderer.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            lineRenderer.gameObject.SetActive(false);
        }

        //
        // Label collision detection
        //

        private void OnTriggerStay(Collider other)
        {      
            var heading = other.transform.position - transform.position;
            heading.z = 0.0f;
            //handling same position issue
            if (heading.magnitude < 0.0001f)
            {
                heading += new Vector3(1.0f, 1.0f, 0);
            }
            //try to "run" away from obstacle
            transform.localPosition -= heading.normalized * Time.unscaledDeltaTime * regroupingSpeed;
            //keep OZ position as 0
            transform.localPosition -= Vector3.forward * transform.localPosition.z;                            
        }

        private void Update()
        {
            if (lastPosition.x != transform.position.x || lastPosition.y != transform.position.y)
            {
                UpdateLine();
            }

            lastPosition = transform.position;
        }

        private void UpdateLine()
        {
            points = lineRenderer.Points;
            //we want only end point + rectangle intersection point
            if (points.Length != 2)
            {
                Array.Resize(ref points, 2);
            }
            //transformating positions into proper spaces
            var worldEndPoint = lineRenderer.transform.TransformPoint(endPoint);
            var rectEndPoint = rectTransform.InverseTransformPoint(worldEndPoint);
            //calculating intersection point
            var point = rectTransform.rect.IntersectionWithRayFromCenter(rectEndPoint);
            var worldStartPoint = rectTransform.TransformPoint(point);
            //finally getting proper point in proper space
            Vector2 rectStartPoint = lineRenderer.transform.InverseTransformPoint(worldStartPoint);
            points[0] = rectStartPoint - offsetVector;
            points[1] = endPoint;
            //forcing refresh
            lineRenderer.SetVerticesDirty();
        }


        public UILineRenderer LineRenderer
        {
            get => lineRenderer;
            set => lineRenderer = value;
        }

        public float RegroupingSpeed
        {
            get => regroupingSpeed;
            set => regroupingSpeed = value;
        }

        public Vector2 EndPoint
        {
            get => endPoint;
            set => endPoint = value;
        }

        public Vector2 OffsetVector
        {
            get => offsetVector;
            set => offsetVector = value;
        }

        private float PointToRectDistance(Rect rect, Vector2 point)
        {
            var dx = Mathf.Max(Mathf.Abs(point.x - rect.center.x) - rect.width / 2, 0);
            var dy = Mathf.Max(Mathf.Abs(point.y - rect.center.y) - rect.height / 2, 0);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}