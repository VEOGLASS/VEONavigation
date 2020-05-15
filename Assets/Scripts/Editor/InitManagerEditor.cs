using UnityEngine;
using UnityEditor;

namespace XploriaAR.Editor
{
    using Toolbox.Editor;
    using Toolbox.Editor.Internal;

    [CustomEditor(typeof(InitManager), true, isFallback = false)]
    public class InitManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty forceSceneProperty;
        private SerializedProperty desiredSceneProperty;
        private SerializedProperty defaultSceneProperty;
        private SerializedProperty availableScenesProperty;
        private SerializedProperty onInitEndProperty;

        private ReorderableList availableScenesList;

        private void OnEnable()
        {
            forceSceneProperty = serializedObject.FindProperty("forceScene");
            desiredSceneProperty = serializedObject.FindProperty("desiredSceneName");
            defaultSceneProperty = serializedObject.FindProperty("defaultScene");
            availableScenesProperty = serializedObject.FindProperty("availableScenes");
            onInitEndProperty = serializedObject.FindProperty("onInitEnd");

            availableScenesList = ToolboxEditorUtility.CreateList(availableScenesProperty, ListStyle.Round);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(forceSceneProperty);
            if (forceSceneProperty.boolValue)
            {
                if (!SceneManager.SceneExists(desiredSceneProperty.stringValue))
                {
                    EditorGUILayout.HelpBox("Scene does not exits. Check available Scenes in Build options.", MessageType.Warning);
                }
                EditorGUILayout.PropertyField(desiredSceneProperty);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(defaultSceneProperty);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
                availableScenesList.DoLayoutList();
            }

            EditorGUILayout.PropertyField(onInitEndProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}