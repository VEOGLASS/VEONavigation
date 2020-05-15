using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace XploriaAR
{
    public class WindManager : MonoBehaviour
    {
        [SerializeField]
        private float direction;

        [Space]

        [SerializeField]
        private Transform headingTransform;

        [SerializeField]
        private TMPro.TextMeshProUGUI headingText;

#if UNITY_EDITOR
        private void OnValidate()
        {
            SetDirection(direction);
        }
#endif

        public void SetDirection(float direction)
        {
            this.direction = Mathf.Clamp(direction, 0, 360);
            if (headingTransform != null)
            {
                var rotation = headingTransform.eulerAngles;
                //TODO: custom rotation axis from inspector
                rotation.z = this.direction;
                headingTransform.eulerAngles = rotation;
            }
            if (headingText == null) return;

            headingText.text = ((int)direction).ToString();
        }

        public float GetDirection()
        {
            return direction;
        }


        public Transform HeadingTransform
        {
            get => headingTransform;
            set => headingTransform = value;
        }

        public TMPro.TextMeshProUGUI HeadingText
        {
            get => headingText;
            set => headingText = value;
        }
    }
}