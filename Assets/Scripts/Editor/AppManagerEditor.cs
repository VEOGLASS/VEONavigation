using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace XploriaAR.Editor
{
    using Toolbox.Editor;

    [CustomEditor(typeof(AppManager), true, isFallback = false)]
    public class AppManagerEditor : ComponentEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Update POIs"))
            {
                Target.UpdateWorldPlaces();
            }
            if (GUILayout.Button("Update Route"))
            {
                Target.UpdateRoutePoints();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public AppManager Target => target as AppManager;
    }
}