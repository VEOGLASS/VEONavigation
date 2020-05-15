using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace XploriaAR
{
    using XploriaAR.UI;

    [DisallowMultipleComponent]
    public sealed class CompassManager : MonoBehaviour
    {
        [SerializeField]
        private bool isActive = true;

        [Space]

        [SerializeField]
        private float step = 5.0f;
        [SerializeField]
        private float bigStep = 90.0f;

        [SerializeField]
        private float radius;

        [Header("Creation"), Separator]

        [SerializeField]
        private UiCompassPole polePrefab;
        [SerializeField]
        private UiCompassPole bigPolePrefab;

        [Space]

        [SerializeField]
        private UiCompassPole desiredCoursePole;
        [SerializeField]
        private UiCompassPole currentCoursePole;


        private void Awake()
        {
            CreatePoles();
        }

        private void OnValidate()
        {
            step = Mathf.Max(step, 0);
            radius = Mathf.Max(radius, 0);
            bigStep = Mathf.Max(bigStep, step);
        }


        private void CreatePoles()
        {
            for (int i = 0; i < 360; i++)
            {
                if (i % step != 0 && i % bigStep != 0) continue;
                var prefab = i % bigStep == 0 ? bigPolePrefab : polePrefab;
                CreatePole(prefab, i, radius);
            }
        }

        private void CreatePole(UiCompassPole prefab, float digit, float radius)
        {
            DisplayPole(Instantiate(prefab, transform), Mathf.Clamp(digit, 0, 360), radius);
        }

        private void DisplayPole(UiCompassPole pole, float digit, float radius)
        {
            var shift = transform.position + transform.forward * radius;
            pole.transform.position = shift;
            pole.transform.LookAt(transform.position);
            pole.transform.RotateAround(transform.position, Vector3.up, digit);
            pole.DigitText.SetText(digit.ToString());
        }


        public void DisplayCurrentCurse(float curse)
        {
            DisplayPole(currentCoursePole, curse, radius);
        }

        public void DisplayDesiredCurse(float curse)
        {
            DisplayPole(desiredCoursePole, curse, radius);
        }
     
        public void SetActive(bool value)
        {
            IsActive = value;
        }


        public bool IsActive
        {
            get => isActive;
            private set
            {
                //TODO: animation
                gameObject.SetActive(value);
                isActive = value;
            }
        }

        public UiCompassPole PolePrefab
        {
            get => polePrefab;
            set => polePrefab = value;
        }

        public UiCompassPole BigPolePrefab
        {
            get => bigPolePrefab;
            set => bigPolePrefab = value;
        }

        public UiCompassPole CurrentCoursePole
        {
            get => currentCoursePole;
            set => currentCoursePole = value;
        }

        public UiCompassPole DesiredCoursePole
        {
            get => desiredCoursePole;
            set => desiredCoursePole = value;
        }
    }
}