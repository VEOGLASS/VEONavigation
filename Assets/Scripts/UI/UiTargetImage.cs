using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XploriaAR.UI
{
    [RequireComponent(typeof(Image))]
    public class UiTargetImage : MonoBehaviour
    {
        private Image Image;

        private void Awake()
        {
            Image = GetComponent<Image>();
        }
    }
}