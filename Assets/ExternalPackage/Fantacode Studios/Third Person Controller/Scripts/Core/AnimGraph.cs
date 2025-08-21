using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace FS_ThirdPerson
{
    public enum Mask { Hand, UpperBody, RightFinger }

    public class AnimGraph : MonoBehaviour
    {
        PlayableGraph graph;
        AnimationPlayableOutput output;
        AnimationMixerPlayable actionMixer;
        AnimationMixerPlayable animatorOutput;
        AnimatorControllerPlayable playableAnimator;

        public Animator animator;
        AvatarMask handMask;
        AvatarMask upperBodyMask;
        AvatarMask rightFingerMask;
        Dictionary<string, object> savedParameters;

        bool maskRemoving = false;
        bool maskAdding = false;
        List<AnimationLayerMixerPlayable> layerMixerList = new List<AnimationLayerMixerPlayable>();

        public void Awake()
        {
            savedParameters = new Dictionary<string, object>();
            CreateGraph();
        }

        private void Update()
        {
            if (animator != null)
            {
                if (playableAnimator.GetSpeed() != animator.speed)
                    playableAnimator.SetSpeed(animator.speed);
            }
        }


        void CreateGraph(AnimatorOverrideController runtimeAnimatorController = null)
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            SetupAnimationGraph();
            CreateDefaultMask();

            playableAnimator = AnimatorControllerPlayable.Create(graph, runtimeAnimatorController == null ? animator.runtimeAnimatorController : runtimeAnimatorController);
            actionMixer = AnimationMixerPlayable.Create(graph, 2);
            animatorOutput = AnimationMixerPlayable.Create(graph, 2);
            animatorOutput.SetInputWeight(0, 1);
            animatorOutput.SetInputWeight(1, 0);
            graph.Connect(playableAnimator, 0, animatorOutput, 0);
            graph.Connect(animatorOutput, 0, actionMixer, 0);

            actionMixer.SetInputWeight(0, 1);
            actionMixer.SetInputWeight(1, 0);
            output.SetSourcePlayable(actionMixer);

            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            graph.Play();
        }

        public void CrossFade(AnimationClip clip, float transitionIn = 0.2f,
            bool transitionBack = true, float transitionOut = 0.2f, float animationSpeed = 1, Action OnComplete = null)
        {
            StartCoroutine(CrossFadeAsync(clip, transitionIn, transitionBack, transitionOut, animationSpeed, OnComplete));
        }

        float transitionTime;
        public IEnumerator CrossFadeAsync(AnimationClip clip, float transitionIn = 0.2f,
            bool transitionBack = true, float transitionOut = 0.2f, float animationSpeed = 1, Action OnComplete = null)
        {
            if (clip == null)
            {
                Debug.Log("no clip found");
                yield break;
            }
            var clipPlayable = AnimationClipPlayable.Create(graph, clip);
            clipPlayable.SetSpeed(animationSpeed);
            var source = actionMixer.GetInput(0);
            actionMixer.DisconnectInput(0);

            var clipMixer = AnimationMixerPlayable.Create(graph, 2);
            clipMixer.ConnectInput(0, source, 0);
            clipMixer.ConnectInput(1, clipPlayable, 0);

            actionMixer.ConnectInput(0, clipMixer, 0);

            actionMixer.SetInputWeight(0, 1);
            actionMixer.SetInputWeight(1, 0);
            var animationTime = transitionIn;

            yield return UpdateWeights(clip, clipMixer, transitionIn, transitionBack, transitionOut,animationSpeed);
            transitionTime = animationTime;
            yield return new WaitUntil(() => actionMixer.GetInput(0).Equals(clipMixer));

            if (transitionBack)
            {
                //TransitionBack();
                var currInput = clipMixer.GetInput(0);
                var currOutput = clipMixer.GetOutput(0);
                clipMixer.DisconnectInput(0);

                if (!currOutput.IsNull())
                {
                    currOutput.DisconnectInput(0);
                    currOutput.ConnectInput(0, currInput, 0);

                    currOutput.SetInputWeight(0, 1);
                    currOutput.SetInputWeight(1, 0);
                }

                clipMixer.Destroy();
                clipPlayable.Destroy();
            }
            OnComplete?.Invoke();
        }

        public bool StopLoopingClip { get; set; }
        public void CrossFadeAndLoop(AnimationClip clip, float transitionIn = 0.2f,
            bool transitionBack=true, float transitionOut = 0.2f)
        {
            StartCoroutine(CrossFadeAndLoopAsync(clip, transitionIn, transitionBack, transitionOut));
        }

        public IEnumerator CrossFadeAndLoopAsync(AnimationClip clip, float transitionIn = 0.2f,
            bool transitionBack = true, float transitionOut = 0.2f)
        {
            StopLoopingClip = false;

            if (clip == null)
            {
                Debug.Log("no clip found");
                yield break;
            }
            var clipPlayable = AnimationClipPlayable.Create(graph, clip);
            var source = actionMixer.GetInput(0);
            actionMixer.DisconnectInput(0);

            var clipMixer = AnimationMixerPlayable.Create(graph, 2);
            clipMixer.ConnectInput(0, source, 0);
            clipMixer.ConnectInput(1, clipPlayable, 0);

            actionMixer.ConnectInput(0, clipMixer, 0);

            actionMixer.SetInputWeight(0, 1);
            actionMixer.SetInputWeight(1, 0);

            float timer = 0f;
            float weight = 0f;

            // Transition In
            while (timer <= transitionIn)
            {
                weight = Mathf.Lerp(0, 1, timer / transitionIn);

                clipMixer.SetInputWeight(0, 1 - weight);
                clipMixer.SetInputWeight(1, weight);

                yield return null;
                timer += Time.deltaTime;
            }

            // Keep looping
            yield return new WaitUntil(() => StopLoopingClip == true);

            // Tranisiton Out
            timer = 0f;
            while (timer <= transitionOut)
            {
                if (transitionBack)
                {
                    weight = Mathf.Lerp(1, 0, timer / transitionIn);

                    clipMixer.SetInputWeight(0, 1 - weight);
                    clipMixer.SetInputWeight(1, weight);
                }

                yield return null;
                timer += Time.deltaTime;
            }

            // Remove animation clip
            var currInput = clipMixer.GetInput(0);
            var currOutput = clipMixer.GetOutput(0);
            clipMixer.DisconnectInput(0);

            if (!currOutput.IsNull())
            {
                currOutput.DisconnectInput(0);
                currOutput.ConnectInput(0, currInput, 0);

                currOutput.SetInputWeight(0, 1);
                currOutput.SetInputWeight(1, 0);
            }

            clipMixer.Destroy();
            clipPlayable.Destroy();
        }

        public void TransitionBack()
        {
            var clipMixer = actionMixer.GetInput(0);
            var clipPlayable = clipMixer.GetInput(1);

            var currInput = clipMixer.GetInput(0);
            var currOutput = clipMixer.GetOutput(0);
            clipMixer.DisconnectInput(0);

            if (!currOutput.IsNull())
            {
                currOutput.DisconnectInput(0);
                currOutput.ConnectInput(0, currInput, 0);

                currOutput.SetInputWeight(0, 1);
                currOutput.SetInputWeight(1, 0);
            }

            clipMixer.Destroy();
            clipPlayable.Destroy();
        }

        public void TransitionBackFully()
        {
            var clipMixer = actionMixer.GetInput(0);
            var clipPlayable = clipMixer.GetInput(1);

            var currOutput = clipMixer.GetOutput(0);
            clipMixer.DisconnectInput(0);

            if (!currOutput.IsNull())
            {
                var animatorOut = playableAnimator.GetOutput(0);
                animatorOut.DisconnectInput(0);

                currOutput.DisconnectInput(0);
                currOutput.ConnectInput(0, playableAnimator, 0);

                currOutput.SetInputWeight(0, 1);
                currOutput.SetInputWeight(1, 0);
            }

            clipMixer.Destroy();
            clipPlayable.Destroy();
        }

        public IEnumerator CrosseFadeOverrideController(AnimatorOverrideController overrideController, float transitionTime)
        {
            if (overrideController == null) yield break;
            playableAnimator = AnimatorControllerPlayable.Create(graph, overrideController);
            animatorOutput = AnimationMixerPlayable.Create(graph, 2);
            animatorOutput.SetInputWeight(0, 1);
            animatorOutput.SetInputWeight(1, 0);
            graph.Connect(playableAnimator, 0, animatorOutput, 0);


            var source = actionMixer.GetInput(0);
            actionMixer.DisconnectInput(0);

            var clipMixer = AnimationMixerPlayable.Create(graph, 2);
            clipMixer.ConnectInput(0, source, 0);
            clipMixer.ConnectInput(1, animatorOutput, 0);

            actionMixer.ConnectInput(0, clipMixer, 0);

            actionMixer.SetInputWeight(0, 1);
            actionMixer.SetInputWeight(1, 0);

            if (transitionTime > 0)
            {
                SaveAnimatorParameters();
                clipMixer.SetInputWeight(0, 1);
                clipMixer.SetInputWeight(1, 0);
                yield return null;
                RestoreAnimatorParameters();
            }

            float timer = 0f;
            float weight = 0f;
            while (timer < transitionTime)
            {
                weight = Mathf.Lerp(0, 1, timer / transitionTime);
                clipMixer.SetInputWeight(0, 1 - weight);
                clipMixer.SetInputWeight(1, weight);
                timer += Time.deltaTime;
                yield return null;
            }
            clipMixer.SetInputWeight(0, 0);
            clipMixer.SetInputWeight(1, 1);
            clipMixer.DisconnectInput(0);
            clipMixer.Destroy();
            actionMixer.DisconnectInput(0);
            graph.Connect(animatorOutput, 0, actionMixer, 0);
            actionMixer.SetInputWeight(0, 1);
            actionMixer.SetInputWeight(1, 0);
            animator.runtimeAnimatorController = overrideController;
        }
        IEnumerator UpdateWeights(AnimationClip clip, AnimationMixerPlayable mixer, float transitionIn,
           bool transitionBack, float transitionOut, float animationSpeed = 1)
        {
            float timer = 0f;
            float weight = 0f;
            bool animationOverriding = false;
            var clipLenth = clip.length / animationSpeed;

            while (timer <= clipLenth)
            {
                if (timer <= transitionIn)
                {
                    weight = Mathf.Lerp(0, 1, timer / transitionIn);
                }
                else if (transitionBack && timer > clipLenth - transitionOut)
                {
                    weight = Mathf.Lerp(1, 0, (timer - (clipLenth - transitionOut)) / transitionOut);
                }

                mixer.SetInputWeight(0, 1 - weight);
                mixer.SetInputWeight(1, weight);

                
                if (!actionMixer.GetInput(0).Equals(mixer) && transitionBack && StopLoopingClip)
                {
                    animationOverriding = true;
                    break;
                }
                yield return null;
                timer += Time.deltaTime;
            }


            if (animationOverriding)
            {
                timer = 0;
                while (timer <= transitionTime + transitionIn)
                {
                    if (timer <= transitionTime)
                    {
                        timer += Time.deltaTime;
                        yield return null;
                        continue;
                    }
                    else
                    {
                        weight = Mathf.Lerp(weight, 1, timer / transitionIn);
                        mixer.SetInputWeight(0, weight);
                        mixer.SetInputWeight(1, 1 - weight);
                        yield return null;
                        timer += Time.deltaTime;
                    }
                }
            }
        }

        public void CrossfadeAvatarMaskAnimation(AnimationClip clip, Mask targetMask = Mask.Hand, bool transitionBack = false, AvatarMask mask = null, float transitionInTime = 0f, bool removeMaskAfterComplete = true, float animationSpeed = 1)
        {
            StartCoroutine(CrossfadeAvatarMaskAnimationAsync(clip, targetMask, transitionBack, mask, transitionInTime, removeMaskAfterComplete, animationSpeed));
        }

        public IEnumerator CrossfadeAvatarMaskAnimationAsync(AnimationClip clip, Mask targetMask = Mask.Hand, bool transitionBack = false, AvatarMask mask = null, float transitionInTime = 0f, bool removeMaskAfterComplete = true, float animationSpeed = 1)
        {
            maskAdding = true;
            yield return new WaitUntil(() => maskRemoving == false);
            if (clip == null)
            {
                maskAdding = false;
                yield break;
            }
            if (mask == null)
            {
                switch (targetMask)
                {
                    case Mask.Hand:
                        mask = handMask;
                        break;
                    case Mask.UpperBody:
                        mask = upperBodyMask;
                        break;
                    case Mask.RightFinger:
                        mask = rightFingerMask;
                        break;
                    default:
                        break;
                }
            }
            layerMixerList.Add(AnimationLayerMixerPlayable.Create(graph, 2));
            var layerMixer = layerMixerList.Last();
            var clipPlayable = AnimationClipPlayable.Create(graph, clip);
            clipPlayable.SetSpeed(animationSpeed);
            var source = actionMixer.GetInput(0);
            actionMixer.DisconnectInput(0);

            layerMixer.ConnectInput(0, source, 0);
            layerMixer.ConnectInput(1, clipPlayable, 0);

            actionMixer.ConnectInput(0, layerMixer, 0);
            actionMixer.SetInputWeight(0, 1);
            actionMixer.SetInputWeight(1, 0);

            layerMixer.SetLayerMaskFromAvatarMask(1, mask);
            layerMixer.SetInputWeight(0, 1f);

            float timer = 0f;
            float weight = 0f;
            transitionInTime = transitionInTime / animationSpeed;
            while (timer < transitionInTime && layerMixer.IsValid() && !maskRemoving)
            {
                weight = Mathf.Lerp(0, 1, timer / transitionInTime);
                layerMixer.SetInputWeight(1, weight);
                timer += Time.deltaTime;
                yield return null;
            }
            if (layerMixer.IsValid())
                layerMixer.SetInputWeight(1, 1);

            if (transitionBack)
            {
                yield return new WaitForSeconds(clip.length/animationSpeed - transitionInTime);
                if (removeMaskAfterComplete)
                    RemoveAvatarMask();
            }
            maskAdding = false;
        }


        public void RemoveAvatarMask(float transitionTime = .2f, bool removeAllLayer = false)
        {
            StartCoroutine(RemoveAvatarMaskAsync(transitionTime, removeAllLayer));
        }
        public IEnumerator RemoveAvatarMaskAsync(float transitionTime = .2f, bool removeAllLayer = false)
        {
            maskRemoving = true;
            yield return new WaitUntil(() => maskAdding == false);
            for (int i = layerMixerList.Count - 1; i >= 0; i--)
            {
                var layerMixer = layerMixerList[i];
                if (layerMixer.IsValid())
                {
                    float timer = 0f;
                    float weight = 0f;
                    while (timer < transitionTime && layerMixer.IsValid() && !maskAdding)
                    {
                        weight = Mathf.Lerp(0, 1, timer / transitionTime);
                        layerMixer.SetInputWeight(1, 1 - weight);
                        timer += Time.deltaTime;
                        yield return null;
                    }
                    if (layerMixer.IsValid())
                    {
                        layerMixer.SetInputWeight(1, 0);
                        var currInput = layerMixer.GetInput(0);
                        var currOutput = layerMixer.GetOutput(0);
                        layerMixer.DisconnectInput(0);

                        if (!currOutput.IsNull())
                        {
                            currOutput.DisconnectInput(0);
                            currOutput.ConnectInput(0, currInput, 0);

                            currOutput.SetInputWeight(0, 1);
                            currOutput.SetInputWeight(1, 0);
                        }
                        layerMixer.Destroy();
                    }
                }

                if (i < layerMixerList.Count)
                    layerMixerList.RemoveAt(i);
                if (!removeAllLayer)
                    break;
            }
            maskRemoving = false;
        }
        void CreateDefaultMask()
        {
            handMask = new AvatarMask();
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                handMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);

            handMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            handMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            handMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightHandIK, true);
            handMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftHandIK, true);
            handMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            handMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);

            upperBodyMask = new AvatarMask();
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                upperBodyMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);

            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightHandIK, true);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftHandIK, true);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            upperBodyMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);

            rightFingerMask = new AvatarMask();
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                rightFingerMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            rightFingerMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
        }


        public IEnumerator CrossfadeBlendAnimationAsync(AnimationClip clip1, AnimationClip clip2, float clip1Weight, float transitionIn = 0.2f, bool transitionBack = true, float transitionOut = 0.2f)
        {
            if (clip1 == null || clip2 == null)
            {
                Debug.Log("no clip found");
                yield break;
            }
            var clipPlayable1 = AnimationClipPlayable.Create(graph, clip1);
            var clipPlayable2 = AnimationClipPlayable.Create(graph, clip2);
            var source = actionMixer.GetInput(0);
            actionMixer.DisconnectInput(0);

            var clipPlayable = AnimationMixerPlayable.Create(graph, 2);
            clipPlayable.ConnectInput(0, clipPlayable1, 0);
            clipPlayable.ConnectInput(1, clipPlayable2, 0);

            clipPlayable.SetInputWeight(0, clip1Weight);
            clipPlayable.SetInputWeight(1, 1 - clip1Weight);

            var clipMixer = AnimationMixerPlayable.Create(graph, 2);
            clipMixer.ConnectInput(0, source, 0);
            clipMixer.ConnectInput(1, clipPlayable, 0);

            actionMixer.ConnectInput(0, clipMixer, 0);

            actionMixer.SetInputWeight(0, 1);
            actionMixer.SetInputWeight(1, 0);

            var animationTime = transitionIn;

            yield return UpdateWeights(clip1, clipMixer, transitionIn, transitionBack, transitionOut,1);
            transitionTime = animationTime;
            yield return new WaitUntil(() => actionMixer.GetInput(0).Equals(clipMixer));

            if (transitionBack)
            {
                var currInput = clipMixer.GetInput(0);
                var currOutput = clipMixer.GetOutput(0);
                clipMixer.DisconnectInput(0);

                if (!currOutput.IsNull())
                {
                    currOutput.DisconnectInput(0);
                    currOutput.ConnectInput(0, currInput, 0);

                    currOutput.SetInputWeight(0, 1);
                    currOutput.SetInputWeight(1, 0);
                }

                clipMixer.Destroy();
                clipPlayable.Destroy();
            }
        }


        public IEnumerator CrossfadeBlendAnimationAsyncLayer(AnimationClip clip1, AnimationClip clip2, float clip1Weight, Mask targetMask = Mask.Hand, bool transitionBack = false, AvatarMask mask = null, float transitionTime = 0f, bool removeMaskAfterComplete = true)
        {
            maskAdding = true;
            yield return new WaitUntil(() => maskRemoving == false);
            if (clip1 == null || clip2 == null)
            {
                Debug.Log("no clip found");
                yield break;
            }
            if (mask == null)
            {
                switch (targetMask)
                {
                    case Mask.Hand:
                        mask = handMask;
                        break;
                    case Mask.UpperBody:
                        mask = upperBodyMask;
                        break;
                    default:
                        break;
                }
            }
            layerMixerList.Add(AnimationLayerMixerPlayable.Create(graph, 2));
            var layerMixer = layerMixerList.Last();


            var clipPlayable1 = AnimationClipPlayable.Create(graph, clip1);
            var clipPlayable2 = AnimationClipPlayable.Create(graph, clip2);
            var source = actionMixer.GetInput(0);
            actionMixer.DisconnectInput(0);

            var clipPlayable = AnimationMixerPlayable.Create(graph, 2);
            clipPlayable.ConnectInput(0, clipPlayable1, 0);
            clipPlayable.ConnectInput(1, clipPlayable2, 0);

            clipPlayable.SetInputWeight(0, clip1Weight);
            clipPlayable.SetInputWeight(1, 1 - clip1Weight);


            layerMixer.ConnectInput(0, source, 0);
            layerMixer.ConnectInput(1, clipPlayable, 0);

            actionMixer.ConnectInput(0, layerMixer, 0);

            actionMixer.SetInputWeight(0, 1);
            actionMixer.SetInputWeight(1, 0);


            layerMixer.SetLayerMaskFromAvatarMask(1, mask);
            layerMixer.SetInputWeight(0, 1f);

            float timer = 0f;
            float weight = 0f;
            while (timer < transitionTime && layerMixer.IsValid() && !maskRemoving)
            {
                weight = Mathf.Lerp(0, 1, timer / transitionTime);
                layerMixer.SetInputWeight(1, weight);
                timer += Time.deltaTime;
                yield return null;
            }
            if (layerMixer.IsValid())
                layerMixer.SetInputWeight(1, 1);

            if (transitionBack)
            {
                yield return new WaitForSeconds(clip1.length - transitionTime);
               
                if (removeMaskAfterComplete)
                    RemoveAvatarMask();
            }
            maskAdding = false;
        }


        void SetupAnimationGraph()
        {
            graph = PlayableGraph.Create(gameObject.name + " graph");
            output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        }
        private void SaveAnimatorParameters()
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (animator.IsParameterControlledByCurve(parameter.nameHash)) continue;
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Float:
                        savedParameters[parameter.name] = animator.GetFloat(parameter.name);
                        break;
                    case AnimatorControllerParameterType.Int:
                        savedParameters[parameter.name] = animator.GetInteger(parameter.name);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        savedParameters[parameter.name] = animator.GetBool(parameter.name);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        savedParameters[parameter.name] = animator.GetBool(parameter.name);
                        break;
                }
            }
        }
        private void RestoreAnimatorParameters()
        {
            foreach (var parameter in savedParameters)
            {
                if (parameter.Value is float)
                {
                    animator.SetFloat(parameter.Key, (float)parameter.Value);
                }
                else if (parameter.Value is int)
                {
                    animator.SetInteger(parameter.Key, (int)parameter.Value);
                }
                else if (parameter.Value is bool)
                {
                    animator.SetBool(parameter.Key, (bool)parameter.Value);
                }
            }
        }
        private void OnDisable()
        {
            graph.Destroy();
        }


        //private void OnGUI()
        //{
        //    var style = new GUIStyle() { fontSize = 24 };
        //     GUILayout.Label(layerMixerList.Count.ToString(), style);
        //     //GUILayout.Label("Weight = " + weight, style);
        //}


    }
}