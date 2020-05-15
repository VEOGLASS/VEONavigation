using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace XploriaAR
{
    [ExecuteInEditMode, RequireComponent(typeof(LineRenderer))]
    public class RouteRenderer : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private LineRenderer lineRenderer;

        [Header("Display"), Separator]

        [SerializeField, ReadOnly]
        private Vector3 displayPosition;
        [SerializeField]
        private Vector3 targetPosition;
        [SerializeField]
        private Vector3 startPosition;

        [Space]

        [SerializeField]
        private int fixedRouteSegmentsCount = 10;

        [Space, Separator, Space]

        [SerializeField]
        private float maxRouteDisplayDistance = 100.0f;

        private void Awake()
        {    
            /*
             * Self-Injection
             */
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void OnValidate()
        {
            fixedRouteSegmentsCount = Mathf.Max(fixedRouteSegmentsCount, 2);
            maxRouteDisplayDistance = Mathf.Max(maxRouteDisplayDistance, 0);
        }

        private void Update()
        {
            SetRoute(startPosition, targetPosition);
        }

        public void SetRoute(Vector3 start, Vector3 end)
        {
            startPosition = start;
            targetPosition = end;
            displayPosition = GetRouteDistance() > maxRouteDisplayDistance ? (end - start).normalized * maxRouteDisplayDistance : end;

            var routePoints = new Vector3[fixedRouteSegmentsCount];
            for (var i = 0; i < fixedRouteSegmentsCount; i++)
            {
                routePoints[i] = Vector3.Lerp(startPosition, displayPosition, (float)i / (fixedRouteSegmentsCount - 1));
            }

            try
            {
                lineRenderer.positionCount = routePoints.Length;          //setting points count
                lineRenderer.SetPositions(routePoints);                   //setting points values
            }
            catch (UnityException)
            {
                ThreadManager.ExecuteInUpdate(() =>
                {
                    lineRenderer.positionCount = routePoints.Length;      //setting points count in UnityThread
                    lineRenderer.SetPositions(routePoints);               //setting points values in UnityThread
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void SetStartPosition(Vector3 position)
        {
            SetRoute(position, targetPosition);
        }

        public void SetTargetPosition(Vector3 position)
        {
            SetRoute(startPosition, position);
        }

        public float GetRouteDistance()
        {
            return (targetPosition - startPosition).magnitude;
        }

        public Vector3 GetStartPosition()
        {
            return startPosition;
        }
        
        public Vector3 GetTargetPosition()
        {
            return targetPosition;
        }
    }
}