﻿using UnityEngine;
using UnityEditor;

namespace Toolbox.Editor
{
    [CustomPropertyDrawer(typeof(HelpAttribute))]
    public class HelpDrawer : DecoratorDrawer
    {
        /// <summary>
        /// Returns current HelpBox height with additional spacing.
        /// </summary>
        /// <returns></returns>
        public override float GetHeight()
        {
            return Style.height + Style.spacing * 2;
        }

        /// <summary>
        /// Draws HelpBox using native method <see cref="EditorGUI.HelpBox(Rect, string, MessageType)"/>.
        /// </summary>
        /// <param name="position"></param>
        public override void OnGUI(Rect position)
        {
            position.height = Style.height;
            position.y += Style.spacing;

            EditorGUI.HelpBox(position, HelpAttribute.Text, (MessageType)HelpAttribute.Type);
        }


        /// <summary>
        /// A wrapper which returns the PropertyDrawer.attribute field as a <see cref="global::HelpAttribute"/>.
        /// </summary>
        private HelpAttribute HelpAttribute => attribute as HelpAttribute;


        /// <summary>
        /// Static representation of help box style.
        /// </summary>
        private static class Style
        {
            internal static readonly float height = EditorGUIUtility.singleLineHeight * 1.25f * 2;
            internal static readonly float spacing = EditorGUIUtility.standardVerticalSpacing;
        }
    }
}