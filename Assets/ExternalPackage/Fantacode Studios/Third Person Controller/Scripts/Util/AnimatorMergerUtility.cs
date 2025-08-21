#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FS_ThirdPerson
{

    public class AnimatorMergerUtility
    {
        public AnimatorController sourceAnimatorController;
        public AnimatorController targetAnimatorController;

        public AnimatorMergerUtility(AnimatorController sourceController, AnimatorController targetController)
        {
            sourceAnimatorController = sourceController;
            targetAnimatorController = targetController;
        }
        public void MergeAnimatorControllers(AnimatorController sourceController, AnimatorController targetController = null)
        {
            if (sourceAnimatorController == null || targetAnimatorController == null)
            {
                Debug.LogError("Both source and target Animator Controllers must be assigned.");
                return;
            }
            if (sourceController == null)
                sourceController = sourceAnimatorController;

            if (targetController == null)
                targetController = targetAnimatorController;

            CopyParameters(sourceController, targetController);

            // Find the "Locomotion" blend tree in the source controller
            BlendTree sourceLocomotion = FindBlendTree(sourceController, "Locomotion");

            if (sourceLocomotion == null)
            {
                Debug.LogError("Locomotion blend tree not found in the source controller.");
                return;
            }

            // Create a new layer in the target controller for the merged content
            AnimatorControllerLayer newLayer = new AnimatorControllerLayer
            {
                name = "Merged Locomotion",
                stateMachine = new AnimatorStateMachine()
            };

            //AssetDatabase.AddObjectToAsset(newLayer.stateMachine, targetController);
            //targetController.AddLayer(newLayer);
            // Recursively copy the state machine structure
            CopyStateMachineStructure(sourceController.layers[0].stateMachine, targetController.layers[0].stateMachine, sourceLocomotion);

            // Save changes
            EditorUtility.SetDirty(targetController);
            AssetDatabase.SaveAssets();

        }

        void CopyStateMachineStructure(AnimatorStateMachine source, AnimatorStateMachine target, Motion targetMotion)
        {
            foreach (var sourceState in source.states)
            {
                //skip if the state already exists
                if (target.states.Any(x => x.state.name == sourceState.state.name)) continue;
                AnimatorState newState = target.AddState(sourceState.state.name, sourceState.position);
                CopyStateProperties(sourceState.state, newState);
                if (sourceState.state.motion == targetMotion)
                {
                    // We found our target motion, replace it with the copied blend tree
                    newState.motion = CopyBlendTree(targetMotion as BlendTree);
                }
            }

            foreach (var sourceChildStateMachine in source.stateMachines)
            {
                AnimatorStateMachine existingChildStateMachine = target.stateMachines.FirstOrDefault(x => x.stateMachine.name == sourceChildStateMachine.stateMachine.name).stateMachine;
                AnimatorStateMachine newChildStateMachine;

                if (existingChildStateMachine == null)
                {
                    newChildStateMachine = target.AddStateMachine(
                        sourceChildStateMachine.stateMachine.name,
                        sourceChildStateMachine.position
                    );
                }
                else
                {
                    newChildStateMachine = existingChildStateMachine;
                }

                CopyStateMachineStructure(sourceChildStateMachine.stateMachine, newChildStateMachine, targetMotion);
            }

            // Copy any state transitions
            foreach (var sourceTransition in source.anyStateTransitions)
            {
                AnimatorState destinationState = GetStateInStateMachine(target, sourceTransition.destinationState.name);
                if (destinationState != null && !target.anyStateTransitions.Any(t => t.destinationState == destinationState))
                {
                    AnimatorStateTransition newTransition = target.AddAnyStateTransition(destinationState);
                    CopyTransitionProperties(sourceTransition, newTransition);
                }
            }

            // Copy transitions for each state
            foreach (var sourceState in source.states)
            {
                AnimatorState targetState = GetStateInStateMachine(target, sourceState.state.name);
                if (targetState != null)
                {
                    foreach (var sourceTransition in sourceState.state.transitions)
                    {
                        AnimatorState destinationState = GetStateInStateMachine(target, sourceTransition.destinationState.name);
                        if (destinationState == null)
                            destinationState = GetStateInStateMachineRecursive(targetAnimatorController.layers[0].stateMachine, sourceTransition.destinationState.name);
                        if (destinationState != null && !targetState.transitions.Any(t => t.destinationState == destinationState))
                        {
                            AnimatorStateTransition newTransition = targetState.AddTransition(destinationState);
                            CopyTransitionProperties(sourceTransition, newTransition);
                        }
                    }
                }
            }

            //// Copy entry transitions
            //foreach (var entryTransition in source.entryTransitions)
            //{
            //    AnimatorState destinationState = GetStateInStateMachine(target, entryTransition.destinationState.name);
            //    if (destinationState != null && !target.entryTransitions.Any(t => t.destinationState == destinationState))
            //    {
            //        AnimatorTransition newEntryTransition = target.AddEntryTransition(destinationState);
            //        CopyEntryTransitionProperties(entryTransition, newEntryTransition);
            //    }
            //}

            // Copy the default state if it exists
            if (source.defaultState != null)
            {
                AnimatorState targetDefaultState = GetStateInStateMachine(target, source.defaultState.name);
                if (targetDefaultState != null)
                {
                    target.defaultState = targetDefaultState;
                }
            }
        }
        void CopyStateMachineStructure1(AnimatorStateMachine source, AnimatorStateMachine target, Motion targetMotion)
        {
            foreach (var sourceState in source.states)
            {
                //skip if the state already exists
                if (target.states.Any(x => x.state.name == sourceState.state.name)) continue;
                AnimatorState newState = target.AddState(sourceState.state.name, sourceState.position);
                CopyStateProperties(sourceState.state, newState);

                if (sourceState.state.motion == targetMotion)
                {
                    // We found our target motion, replace it with the copied blend tree
                    newState.motion = CopyBlendTree(targetMotion as BlendTree);
                }
            }

            foreach (var sourceChildStateMachine in source.stateMachines)
            {
                AnimatorStateMachine newChildStateMachine = target.AddStateMachine(
                    sourceChildStateMachine.stateMachine.name,
                    sourceChildStateMachine.position
                );
                CopyStateMachineStructure(sourceChildStateMachine.stateMachine, newChildStateMachine, targetMotion);
            }

            // Copy transitions
            foreach (var sourceTransition in source.anyStateTransitions)
            {
                AnimatorStateTransition newTransition = target.AddAnyStateTransition(GetStateInStateMachine(target, sourceTransition.destinationState.name));
                CopyTransitionProperties(sourceTransition, newTransition);
            }

            foreach (var sourceState in source.states)
            {
                AnimatorState targetState = GetStateInStateMachine(target, sourceState.state.name);
                foreach (var sourceTransition in sourceState.state.transitions)
                {
                    AnimatorStateTransition newTransition = targetState.AddTransition(GetStateInStateMachine(target, sourceTransition.destinationState.name));
                    CopyTransitionProperties(sourceTransition, newTransition);
                }
            }
        }

        void CopyStateProperties(AnimatorState source, AnimatorState target)
        {
            target.speed = source.speed;
            target.cycleOffset = source.cycleOffset;
            target.mirror = source.mirror;
            target.mirrorParameterActive = source.mirrorParameterActive;
            target.mirrorParameter = source.mirrorParameter;
            target.iKOnFeet = source.iKOnFeet;
            target.writeDefaultValues = source.writeDefaultValues;
            target.tag = source.tag;

            if (source.motion is BlendTree)
            {
                target.motion = CopyBlendTree(source.motion as BlendTree);
            }
            else
            {
                target.motion = source.motion;
            }

            // Copy behaviors
            foreach (var behavior in source.behaviours)
            {
                var newBehavior = target.AddStateMachineBehaviour(behavior.GetType());
                EditorUtility.CopySerialized(behavior, newBehavior);
            }
        }

        void CopyTransitionProperties(AnimatorStateTransition source, AnimatorStateTransition target)
        {
            target.duration = source.duration;
            target.offset = source.offset;
            target.interruptionSource = source.interruptionSource;
            target.orderedInterruption = source.orderedInterruption;
            target.exitTime = source.exitTime;
            target.hasExitTime = source.hasExitTime;
            target.hasFixedDuration = source.hasFixedDuration;
            target.canTransitionToSelf = source.canTransitionToSelf;

            // Copy conditions
            foreach (var condition in source.conditions)
            {
                target.AddCondition(condition.mode, condition.threshold, condition.parameter);
            }
        }

        BlendTree CopyBlendTree(BlendTree source)
        {
            BlendTree newTree = new BlendTree
            {
                name = source.name,
                blendType = source.blendType,
                blendParameter = source.blendParameter,
                blendParameterY = source.blendParameterY,
                minThreshold = source.minThreshold,
                maxThreshold = source.maxThreshold,
                useAutomaticThresholds = source.useAutomaticThresholds
            };

            for (int i = 0; i < source.children.Length; i++)
            {
                var child = source.children[i];
                Motion childMotion;

                if (child.motion is BlendTree childBlendTree)
                {
                    childMotion = CopyBlendTree(childBlendTree);
                }
                else
                {
                    childMotion = child.motion;
                }

                // Copy position (only applicable for 2D blend types)
                if (newTree.blendType == BlendTreeType.SimpleDirectional2D || newTree.blendType == BlendTreeType.FreeformDirectional2D || newTree.blendType == BlendTreeType.FreeformCartesian2D)
                {
                    newTree.AddChild(childMotion, child.position);

                }
                else
                    newTree.AddChild(childMotion, child.threshold);

                SerializedObject serializedTree = new SerializedObject(newTree);
                //PrintAllProperties(serializedTree);
                SerializedProperty childrenProp = serializedTree.FindProperty("m_Childs");
                if (childrenProp != null && i < childrenProp.arraySize)
                {
                    SerializedProperty childProp = childrenProp.GetArrayElementAtIndex(i);
                    childProp.FindPropertyRelative("m_TimeScale").floatValue = child.timeScale;
                    childProp.FindPropertyRelative("m_CycleOffset").floatValue = child.cycleOffset;
                    childProp.FindPropertyRelative("m_Mirror").boolValue = child.mirror;
                }
                serializedTree.ApplyModifiedProperties();
            }

            AssetDatabase.AddObjectToAsset(newTree, targetAnimatorController);
            newTree.hideFlags = HideFlags.HideInHierarchy;

            return newTree;
        }

        BlendTree FindBlendTree(AnimatorController controller, string name)
        {
            foreach (var layer in controller.layers)
            {
                var blendTree = FindBlendTreeRecursive(layer.stateMachine, name);
                if (blendTree != null)
                {
                    return blendTree;
                }
            }
            return null;
        }

        BlendTree FindBlendTreeRecursive(AnimatorStateMachine stateMachine, string name)
        {
            foreach (var state in stateMachine.states)
            {
                if (state.state.motion is BlendTree blendTree)
                {
                    if (state.state.name == name)
                    {
                        return blendTree;
                    }
                    var nestedResult = FindBlendTreeRecursive(blendTree, name);
                    if (nestedResult != null)
                    {
                        return nestedResult;
                    }
                }
            }

            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                var result = FindBlendTreeRecursive(childStateMachine.stateMachine, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        BlendTree FindBlendTreeRecursive(BlendTree blendTree, string name)
        {
            foreach (var child in blendTree.children)
            {
                if (child.motion is BlendTree childBlendTree)
                {
                    if (childBlendTree.name == name)
                    {
                        return childBlendTree;
                    }
                    var nestedResult = FindBlendTreeRecursive(childBlendTree, name);
                    if (nestedResult != null)
                    {
                        return nestedResult;
                    }
                }
            }
            return null;
        }

        AnimatorState GetStateInStateMachine(AnimatorStateMachine stateMachine, string stateName)
        {
            foreach (var state in stateMachine.states)
            {
                if (state.state.name == stateName)
                {
                    return state.state;
                }
            }
            return null;
        }
        AnimatorState GetStateInStateMachineRecursive(AnimatorStateMachine stateMachine, string stateName)
        {
            foreach (var state in stateMachine.states)
            {
                if (state.state.name == stateName)
                {
                    return state.state;
                }
            }
            foreach (var item in stateMachine.stateMachines)
            {
                var state = GetStateInStateMachineRecursive(item.stateMachine, stateName);
                if (state != null) return state;
            }
            return null;
        }

        void CopyParameters(AnimatorController source, AnimatorController target)
        {
            foreach (var parameter in source.parameters)
            {
                // Check if the parameter already exists in the target controller
                if (!HasParameter(target, parameter.name))
                {
                    target.AddParameter(parameter.name, parameter.type);
                }
            }
        }

        bool HasParameter(AnimatorController controller, string parameterName)
        {
            foreach (var parameter in controller.parameters)
            {
                if (parameter.name == parameterName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
#endif