using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace XploriaAR.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UiNmeaDataText : MonoBehaviour
    {
        private TextMeshProUGUI text;

        [SerializeField]
        private float value;

        [Space]

        [SerializeField, Range(0, 100)]
        private int suffixSize = 60;
        [SerializeField]
        private string suffix;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        public void SetValue(float value)
        {
            this.value = value;
            ValueText = value.ToString();
        }

        public float GetValue()
        {
            return value;
        }

        public int SuffixSize
        {
            get => suffixSize;
            set => suffixSize = Mathf.Clamp(value, 0, 100);
        }

        public string Suffix
        {
            get => suffix;
            set => suffix = value;
        }

        public string ValueText
        {
            get => text.text;
            private set => text.text = value + "<size=" + suffixSize + "%>" + suffix;
        }
    }
}