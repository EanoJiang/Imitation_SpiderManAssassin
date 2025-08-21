using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace FS_ThirdPerson
{

    [CustomEditor(typeof(CameraController))]
    public class CameraControllerEditor : Editor
    {
        SerializedProperty followTarget;
        SerializedProperty defaultSettings;
        SerializedProperty advancedCameraRotation;
        SerializedProperty collisionLayers;
        SerializedProperty lockCursor;
        SerializedProperty distanceSmoothTime;
        SerializedProperty distanceSmoothTimeWhenOcluded;
        SerializedProperty framingSmoothTime;
        SerializedProperty collisionPadding;
        SerializedProperty overrideCameraSettings;
        SerializedProperty nearClipPlane;

        private bool showAdvancedSettings = true;
        private bool showCollisionSettings = true;

        private void OnEnable()
        {
            followTarget = serializedObject.FindProperty("followTarget");
            defaultSettings = serializedObject.FindProperty("defaultSettings");
            advancedCameraRotation = serializedObject.FindProperty("advancedCameraRotation");
            collisionLayers = serializedObject.FindProperty("collisionLayers");
            lockCursor = serializedObject.FindProperty("lockCursor");
            distanceSmoothTime = serializedObject.FindProperty("distanceSmoothTime");
            distanceSmoothTimeWhenOcluded = serializedObject.FindProperty("distanceSmoothTimeWhenOcluded");
            framingSmoothTime = serializedObject.FindProperty("framingSmoothTime");
            collisionPadding = serializedObject.FindProperty("collisionPadding");
            overrideCameraSettings = serializedObject.FindProperty("overrideCameraSettings");
            nearClipPlane = serializedObject.FindProperty("nearClipPlane");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Camera Controller Settings", EditorStyles.boldLabel);

            // Target and Default Settings
            EditorGUILayout.PropertyField(followTarget, new GUIContent("Follow Target", "Target to follow"));
            EditorGUILayout.PropertyField(defaultSettings, new GUIContent("Default Settings", "Default settings for the camera. You can override these based on the player's state."));

            EditorGUILayout.Space();

            // Advanced Settings Foldout
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Settings", true);
            if (showAdvancedSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(advancedCameraRotation, new GUIContent("Advanced Camera Rotation", "If turned on, the camera will rotate when the player moves sideways."));
                EditorGUILayout.PropertyField(lockCursor, new GUIContent("Lock Cursor", "This value must be set before starting play mode. It cannot be changed while the game is running."));
                EditorGUILayout.PropertyField(nearClipPlane, new GUIContent("Near Clip Plane", "This value must be set before starting play mode. It cannot be changed while the game is running."));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Collision Settings Foldout
            showCollisionSettings = EditorGUILayout.Foldout(showCollisionSettings, "Collision Settings", true);
            if (showCollisionSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(collisionLayers, new GUIContent("Collision Layers", "Layers to check for collision"));
                EditorGUILayout.PropertyField(collisionPadding, new GUIContent("Collision Padding", "Padding to avoid clipping into objects."));
                EditorGUILayout.PropertyField(distanceSmoothTimeWhenOcluded, new GUIContent("Smooth Time (Occluded)", "Smooth time when the camera distance changes due to a collision"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Camera Distance and Framing Settings
            EditorGUILayout.LabelField("Distance and Framing Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(distanceSmoothTime, new GUIContent("Distance Smooth Time", "Smooth time to use when camera distance is changed."));
            EditorGUILayout.PropertyField(framingSmoothTime, new GUIContent("Framing Smooth Time", "Smooth time to use when the framing offset is changed."));

            EditorGUILayout.Space();

            // Override Camera Settings Foldout
            EditorGUILayout.PropertyField(overrideCameraSettings, new GUIContent("Override Settings", "This can be used to override the camera settings for different states."), true);

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}