using UnityEngine;
using UnityEditor;

namespace FS_ThirdPerson
{

    [CustomEditor(typeof(LocomotionController))]
    public class LocomotionControllerEditor : Editor
    {
        // Serialized properties
        SerializedProperty sprintSpeed;
        SerializedProperty runSpeed;
        SerializedProperty walkSpeed;
        SerializedProperty rotationSpeed;

        SerializedProperty acceleration;
        SerializedProperty deceleration;

        SerializedProperty enableSprint;
        SerializedProperty setDefaultStateToRunning;

        SerializedProperty useMultiDirectionalAnimation;
        SerializedProperty playerDirectionBlend;
        SerializedProperty faceCameraForwardWhenIdle;
        SerializedProperty speedMultiplier;
        SerializedProperty sprintDirectionThreshold;
        SerializedProperty forwardHipRotationBlend;
        SerializedProperty rotateHipForBackwardAnimation;
        SerializedProperty backwardHipRotationBlend;

        SerializedProperty verticalJump;
        SerializedProperty timeToJump;
        SerializedProperty jumpMoveSpeed;

        SerializedProperty preventFallingFromLedge;
        SerializedProperty slidingMovementThresholdFromLedge;
        SerializedProperty preventLedgeRotation;
        SerializedProperty preventWallSlide;
        SerializedProperty enableBalanceWalk;
        SerializedProperty balanceWalkDetectionType;

        SerializedProperty enableTurningAnim;
        SerializedProperty playQuickTurnAnimation;
        SerializedProperty QuickTurnThreshhold;
        SerializedProperty playQuickStopAnimation;
        SerializedProperty runToStopThreshhold;

        SerializedProperty groundCheckRadius;
        SerializedProperty groundCheckOffset;
        SerializedProperty groundLayer;

        private void OnEnable()
        {
            // Fetch all serialized properties
            sprintSpeed = serializedObject.FindProperty("sprintSpeed");
            runSpeed = serializedObject.FindProperty("runSpeed");
            walkSpeed = serializedObject.FindProperty("walkSpeed");
            rotationSpeed = serializedObject.FindProperty("rotationSpeed");

            acceleration = serializedObject.FindProperty("acceleration");
            deceleration = serializedObject.FindProperty("deceleration");

            enableSprint = serializedObject.FindProperty("enableSprint");
            setDefaultStateToRunning = serializedObject.FindProperty("setDefaultStateToRunning");

            useMultiDirectionalAnimation = serializedObject.FindProperty("useMultiDirectionalAnimation");
            playerDirectionBlend = serializedObject.FindProperty("playerDirectionBlend");
            faceCameraForwardWhenIdle = serializedObject.FindProperty("faceCameraForwardWhenIdle");
            speedMultiplier = serializedObject.FindProperty("speedMultiplier");
            sprintDirectionThreshold = serializedObject.FindProperty("sprintDirectionThreshold");
            forwardHipRotationBlend = serializedObject.FindProperty("forwardHipRotationBlend");
            rotateHipForBackwardAnimation = serializedObject.FindProperty("rotateHipForBackwardAnimation");
            backwardHipRotationBlend = serializedObject.FindProperty("backwardHipRotationBlend");

            verticalJump = serializedObject.FindProperty("verticalJump");
            timeToJump = serializedObject.FindProperty("timeToJump");
            jumpMoveSpeed = serializedObject.FindProperty("jumpMoveSpeed");

            preventFallingFromLedge = serializedObject.FindProperty("preventFallingFromLedge");
            slidingMovementThresholdFromLedge = serializedObject.FindProperty("slidingMovementThresholdFromLedge");
            preventLedgeRotation = serializedObject.FindProperty("preventLedgeRotation");
            preventWallSlide = serializedObject.FindProperty("preventWallSlide");
            enableBalanceWalk = serializedObject.FindProperty("enableBalanceWalk");
            balanceWalkDetectionType = serializedObject.FindProperty("balanceWalkDetectionType");

            enableTurningAnim = serializedObject.FindProperty("enableTurningAnim");
            playQuickTurnAnimation = serializedObject.FindProperty("playQuickTurnAnimation");
            QuickTurnThreshhold = serializedObject.FindProperty("QuickTurnThreshhold");
            playQuickStopAnimation = serializedObject.FindProperty("playQuickStopAnimation");
            runToStopThreshhold = serializedObject.FindProperty("runToStopThreshhold");

            groundCheckRadius = serializedObject.FindProperty("groundCheckRadius");
            groundCheckOffset = serializedObject.FindProperty("groundCheckOffset");
            groundLayer = serializedObject.FindProperty("groundLayer");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(sprintSpeed);
            EditorGUILayout.PropertyField(runSpeed);
            EditorGUILayout.PropertyField(walkSpeed);
            EditorGUILayout.PropertyField(rotationSpeed);

            EditorGUILayout.PropertyField(acceleration);
            EditorGUILayout.PropertyField(deceleration);

            EditorGUILayout.PropertyField(enableSprint);
            EditorGUILayout.PropertyField(setDefaultStateToRunning);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(useMultiDirectionalAnimation);
            if (useMultiDirectionalAnimation.boolValue)
            {
                EditorGUILayout.PropertyField(playerDirectionBlend);
                EditorGUILayout.PropertyField(faceCameraForwardWhenIdle);
                EditorGUILayout.PropertyField(speedMultiplier);
                EditorGUILayout.PropertyField(sprintDirectionThreshold);
                EditorGUILayout.PropertyField(forwardHipRotationBlend);
                EditorGUILayout.PropertyField(rotateHipForBackwardAnimation);
                EditorGUILayout.PropertyField(backwardHipRotationBlend);
                EditorGUILayout.Space(10);
            }

            EditorGUILayout.PropertyField(verticalJump);
            if (verticalJump.boolValue)
            {
                EditorGUILayout.PropertyField(timeToJump);
                EditorGUILayout.PropertyField(jumpMoveSpeed);
                EditorGUILayout.Space(10);
            }
            EditorGUILayout.PropertyField(preventFallingFromLedge);
            if (preventFallingFromLedge.boolValue)
            {
                EditorGUILayout.PropertyField(slidingMovementThresholdFromLedge);
                EditorGUILayout.PropertyField(preventLedgeRotation);
            }
            EditorGUILayout.PropertyField(preventWallSlide);
            EditorGUILayout.PropertyField(enableBalanceWalk);
            if(enableBalanceWalk.boolValue)
                EditorGUILayout.PropertyField(balanceWalkDetectionType);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(enableTurningAnim);
            EditorGUILayout.PropertyField(playQuickTurnAnimation);
            if(playQuickTurnAnimation.boolValue)
                EditorGUILayout.PropertyField(QuickTurnThreshhold);
            EditorGUILayout.PropertyField(playQuickStopAnimation);
            if(playQuickStopAnimation.boolValue)
                EditorGUILayout.PropertyField(runToStopThreshhold);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(groundCheckRadius);
            EditorGUILayout.PropertyField(groundCheckOffset);
            EditorGUILayout.PropertyField(groundLayer);

            serializedObject.ApplyModifiedProperties();
        }
    }
}