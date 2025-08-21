using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace FS_GrapplingSystem {

    [CustomEditor(typeof(GrapplingHookController))]
    public class GrapplingHookControllerEditor : Editor
    {
        SerializedProperty enableHook;
        SerializedProperty mode;
        SerializedProperty maxSwingAngleLimit;
        SerializedProperty swingOnlyIfHookPointExists;

        SerializedProperty hookInput;
        SerializedProperty hookInputButton;
        SerializedProperty maxDistance;
        SerializedProperty maxSwingDistance;
        SerializedProperty minDistance;
        SerializedProperty grapplingHookMoveSpeed;
        SerializedProperty swingActionMoveSpeed;
        SerializedProperty cameraShakeAmount;
        SerializedProperty cameraShakeDuration;
        SerializedProperty crosshairPrefab;
        SerializedProperty crosshairSize;
        SerializedProperty ropeStartPoint;
        SerializedProperty debug;
        SerializedProperty RopeReleased;
        SerializedProperty RopeHooked;
        SerializedProperty movementStarted;
        SerializedProperty TargetPointReached;
        SerializedProperty ropeThrowingClip;
        SerializedProperty animationSpeed;

        SerializedProperty hookObject;
        SerializedProperty ropeRadius;
        SerializedProperty ropeMaterial;
        SerializedProperty ropeThrowSpeed;
        SerializedProperty ropeResolution;
        SerializedProperty spiralRadius;
        SerializedProperty spiralFrequency;

        private void OnEnable()
        {
            enableHook = serializedObject.FindProperty("enableHook");
            mode = serializedObject.FindProperty("mode");
            maxSwingAngleLimit = serializedObject.FindProperty("maxSwingAngleLimit");
            swingOnlyIfHookPointExists = serializedObject.FindProperty("swingOnlyIfHookPointExists");

            hookInput = serializedObject.FindProperty("hookInput");
            hookInputButton = serializedObject.FindProperty("hookInputButton");
            maxDistance = serializedObject.FindProperty("maxDistance");
            maxSwingDistance = serializedObject.FindProperty("maxSwingDistance");
            minDistance = serializedObject.FindProperty("minDistance");
            grapplingHookMoveSpeed = serializedObject.FindProperty("grapplingHookMoveSpeed");
            swingActionMoveSpeed = serializedObject.FindProperty("swingActionMoveSpeed");

            cameraShakeAmount = serializedObject.FindProperty("cameraShakeAmount");
            cameraShakeDuration = serializedObject.FindProperty("cameraShakeDuration");

            crosshairPrefab = serializedObject.FindProperty("crosshairPrefab");
            crosshairSize = serializedObject.FindProperty("crosshairSize");

            debug = serializedObject.FindProperty("debug");

            hookObject = serializedObject.FindProperty("hookObject");
            ropeStartPoint = serializedObject.FindProperty("ropeStartPoint");

            RopeReleased = serializedObject.FindProperty("RopeReleased");
            RopeHooked = serializedObject.FindProperty("RopeHooked");
            movementStarted = serializedObject.FindProperty("movementStarted");
            TargetPointReached = serializedObject.FindProperty("TargetPointReached");

            ropeThrowingClip = serializedObject.FindProperty("ropeThrowingClip");
            animationSpeed = serializedObject.FindProperty("animationSpeed");

            ropeRadius = serializedObject.FindProperty("ropeRadius");
            ropeMaterial = serializedObject.FindProperty("ropeMaterial");
            ropeThrowSpeed = serializedObject.FindProperty("ropeThrowSpeed");
            ropeResolution = serializedObject.FindProperty("ropeResolution");
            spiralRadius = serializedObject.FindProperty("spiralRadius");
            spiralFrequency = serializedObject.FindProperty("spiralFrequency");
        }

        int selectedToolbarIndex = 0; 

        string[] toolbarOptions1 = new string[] { "Main", "Rope Settings", "Camera Settings" };
        string[] toolbarOptions2 = new string[] { "Crosshair", "Events", "Debug" }; 

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Main Settings
            selectedToolbarIndex = GUILayout.Toolbar(selectedToolbarIndex, toolbarOptions1,GUILayout.Height(25));

            selectedToolbarIndex = GUILayout.Toolbar(selectedToolbarIndex - toolbarOptions1.Length, toolbarOptions2, GUILayout.Height(25)) + toolbarOptions1.Length;

            switch (selectedToolbarIndex)
            {
                case 0: // Grappling Hook
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(enableHook, new GUIContent("Enable Hook", "A boolean that enables or disables the grappling hook functionality"));
                    EditorGUILayout.PropertyField(hookInput, new GUIContent("Input", "The key to activate the grappling hook."));
                    EditorGUILayout.PropertyField(hookInputButton, new GUIContent("Hook Input Button", "The button to activate the grappling hook."));

                    EditorGUILayout.PropertyField(mode);
                    if (mode.enumValueIndex != (int)Modes.GrapplingHookOnly)
                    {
                        EditorGUILayout.PropertyField(maxSwingAngleLimit);
                        EditorGUILayout.PropertyField(swingOnlyIfHookPointExists);
                    }

                    EditorGUILayout.PropertyField(ropeThrowingClip, new GUIContent("Rope Throwing Clip", "The animation clip used when the rope is being thrown for the hook."));
                    EditorGUILayout.PropertyField(animationSpeed, new GUIContent("Animation Speed", "Animation playing speed"));
                    EditorGUILayout.PropertyField(maxDistance, new GUIContent("Max Distance", "The maximum distance the grappling hook can reach."));
                    EditorGUILayout.PropertyField(maxSwingDistance, new GUIContent("Max Swing Distance", "The maximum distance to swing"));
                    EditorGUILayout.PropertyField(minDistance, new GUIContent("Min Distance", "The minimum distance required to activate the grappling hook."));
                    EditorGUILayout.PropertyField(grapplingHookMoveSpeed);
                    if (mode.enumValueIndex != (int)Modes.GrapplingHookOnly)
                        EditorGUILayout.PropertyField(swingActionMoveSpeed);
                    break;

                case 1: // Rope Settings
                    EditorGUILayout.Space();
                    
                   
                    EditorGUILayout.PropertyField(ropeRadius);
                    EditorGUILayout.PropertyField(ropeMaterial);
                    EditorGUILayout.PropertyField(ropeThrowSpeed);
                    EditorGUILayout.PropertyField(ropeResolution);
                    EditorGUILayout.PropertyField(spiralRadius);
                    EditorGUILayout.PropertyField(spiralFrequency);
                    EditorGUILayout.PropertyField(ropeStartPoint, new GUIContent("Rope Start Point", "The starting point of the rope. If not set, the right hand will be used as the default."));
                    EditorGUILayout.PropertyField(hookObject);
                    break;

                case 2: // Camera Settings
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(cameraShakeAmount, new GUIContent("Shake Amount", "The intensity of the camera shake when the hook is pulled."));
                    EditorGUILayout.PropertyField(cameraShakeDuration, new GUIContent("Shake Duration", "The duration of the camera shake when the hook is pulled."));
                    break;

                case 3: // Crosshair Settings
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(crosshairPrefab, new GUIContent("Crosshair Prefab", "The crosshair to display when aiming the grappling hook."));
                    EditorGUILayout.PropertyField(crosshairSize, new GUIContent("Crosshair Size", "The size of the crosshair."));
                    break;

                case 4: // Events
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(RopeReleased, new GUIContent("Rope Released", "Event triggered when the rope is released."));
                    EditorGUILayout.PropertyField(RopeHooked, new GUIContent("Rope Hooked", "Event triggered when the rope hooks to a target."));
                    EditorGUILayout.PropertyField(movementStarted, new GUIContent("Movement Started", "Event triggered when the player is pulled by the rope."));
                    EditorGUILayout.PropertyField(TargetPointReached, new GUIContent("Target Point Reached", "Event triggered when the player reaches the target point."));
                    break;

                case 5: // Debug Settings
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(debug, new GUIContent("Debug Mode"));
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
