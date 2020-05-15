using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XploriaAR.UI
{
    public class UiCompassPole : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI digitText;
        
        public TMPro.TextMeshProUGUI DigitText
        {
            get => digitText;
            set => digitText = value;
        }
    }
}