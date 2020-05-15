using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace XploriaAR
{
    using TMPro;

    [CreateAssetMenu(menuName = "XploriaAR/PlaceSettings[POI]", order = 2)]
    public class PlaceSettings : ScriptableObject
    {
        [Separator, Header("Options")]

        public bool useTypeIcon = true;
        public bool useFixedScale = true;
        public bool useFixedHeight;
        public bool usePointingLine = true;

        [Space]

        public int maxNameCharacters = 30;

        [Space, Separator]
    
        [ConditionalField("useFixedSize", true)]
        public float fixedSize = 0.001f;
        [ConditionalField("useFixedHeight", true)]
        public float fixedHeight = 2f;

        [Space]

        public float nameTextFontSize = 1.0f;
        public float typeTextFontSize = 0.75f;
        public float distTextFontSize = 0.75f;

        [Space]

        public TextAlignmentOptions nameTextOption;
        public TextAlignmentOptions distTextOption;
        public TextAlignmentOptions typeTextOption;

        [Space]

        public Color labelColor;
        public Color outlineColor;

        [Separator, Header("Icon")]

        public Color iconColor;

        [Space]

        public Vector2 iconSize;

        [Space]

        public Vector2 iconAnchorMin;
        public Vector2 iconAnchorMax;
    }
}