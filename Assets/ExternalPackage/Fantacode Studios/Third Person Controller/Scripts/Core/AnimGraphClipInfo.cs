using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FS_ThirdPerson
{
    [Serializable]
    public class AnimGraphClipInfo : ISerializationCallbackReceiver
    {
        public AnimationClip clip;
        public bool customAnimationSpeed;
        public bool useAsGlobalTimeScale = false;
        public ValueModifier speedModifier = new ValueModifier() { defaultCurveRect = new Rect(0, 0, 1, 3) };
        public bool useGravity = true;
        public ValueModifier gravityModifier = new ValueModifier() { defaultCurveRect = new Rect(0, -2, 1, 4) };
        public bool showDetails;
        public Vector2 TranistionInAndOut = new Vector2(0.2f, 0.2f);
        public List<AnimationEventData> events = new List<AnimationEventData>();
        public List<AnimationEventData> onEndAnimation = new List<AnimationEventData>();

        public float frameRate => clip?.frameRate ?? 30;
        public float length => clip?.length ?? 0;

        public static implicit operator AnimationClip(AnimGraphClipInfo animInfo)
        {
            return animInfo?.clip;
        }

        [SerializeField, HideInInspector]
        private bool serialized;

        public void OnAfterDeserialize() 
        {
            if (!serialized)
            {
                TranistionInAndOut = new Vector2(0.2f, 0.2f);
                useGravity = true;
                speedModifier = new ValueModifier() { defaultCurveRect = new Rect(0, 0, 1, 3) };
                gravityModifier = new ValueModifier() { defaultCurveRect = new Rect(0, -2, 1, 4) };
            }
        }

        public void OnBeforeSerialize()
        {
            if (serialized) return;
            serialized = true;
        }

        public static Type FindTypeByName(string className)
        {
            return TypeCache.FindTypeByName(className);
        }
    }

    [Serializable]
    public class ValueModifier : ISerializationCallbackReceiver
    {
        public enum ModifierType { Constant, Curve }

        public ModifierType currentModifierType;
        public float constant = 1;
        public AnimationCurve curve = AnimationCurve.Linear(0, 1, 1, 1);
        public Rect defaultCurveRect = new Rect(0, -1, 1, 3);

        [SerializeField, HideInInspector]
        private bool serialized;

        public float GetValue(float time = 0)
        {
            if (currentModifierType == ModifierType.Constant)
                return constant;
            else if (currentModifierType == ModifierType.Curve)
                return curve.Evaluate(time);

            return 1f;
        }

        public void OnAfterDeserialize()
        {
            if (!serialized)
            {
                constant = 1;
                curve = AnimationCurve.Linear(0, 1, 1, 1);
            }
        }

        public void OnBeforeSerialize()
        {
            if (serialized) return;
            serialized = true;
        }
    }

    [Serializable]
    public class AnimationEventData
    {
        [Serializable]
        public class ParameterData
        {
            public enum ParameterType
            {
                Integer,
                Float,
                String,
                Boolean,
                Vector2,
                Vector3,
                GameObject,
                HumanBodyBone,
                Component,
                Color,
                AnimationCurve,
                AudioClip,
                Texture,
                Material,
                Sprite,
                LayerMask,
                Object,
                CameraSettings
            }

            public ParameterType type;
            public int intValue;
            public float floatValue;
            public string stringValue;
            public bool boolValue;
            public Vector2 vector2Value;
            public Vector3 vector3Value;
            public GameObject gameObjectValue;
            public HumanBodyBones humanBodyBoneValue;
            public Component componentValue;
            public Color colorValue;
            public AnimationCurve animationCurveValue;
            public AudioClip audioClipValue;
            public Texture textureValue;
            public Material materialValue;
            public Sprite spriteValue;
            public LayerMask layerMaskValue;
            public UnityEngine.Object _object;
            public CameraSettings cameraSettings;

            public object GetValue()
            {
                return type switch
                {
                    ParameterType.Integer => intValue,
                    ParameterType.Float => floatValue,
                    ParameterType.String => stringValue,
                    ParameterType.Boolean => boolValue,
                    ParameterType.Vector2 => vector2Value,
                    ParameterType.Vector3 => vector3Value,
                    ParameterType.GameObject => gameObjectValue,
                    ParameterType.HumanBodyBone => humanBodyBoneValue,
                    ParameterType.Component => componentValue,
                    ParameterType.Color => colorValue,
                    ParameterType.AnimationCurve => animationCurveValue,
                    ParameterType.AudioClip => audioClipValue,
                    ParameterType.Texture => textureValue,
                    ParameterType.Material => materialValue,
                    ParameterType.Sprite => spriteValue,
                    ParameterType.LayerMask => layerMaskValue,
                    ParameterType.Object => _object,
                    ParameterType.CameraSettings => cameraSettings,
                    _ => null
                };
            }
            public string parameterName;
        }



        [Tooltip("Normalized time from 0 to 1")]
        [Range(0, 1)]
        public float normalizedTime;

        [Tooltip("Reference to the target Unity Object")]
        public UnityEngine.Object targetObject;

        [Tooltip("Name of the method to invoke")]
        public string methodName;

        [Tooltip("List of parameters for method invocation")]
        public List<ParameterData> parameters = new List<ParameterData>();
        public List<string> methodInfos = new List<string>();
        public bool foldOut;

        public bool isGameObject;
        public string targetName;

        public AnimationClip clip;

        public object[] parametersList => parameters.Select(p => p.GetValue()).ToArray();



        public void InvokeCustomAnimationEvent(GameObject g)
        {
            if (targetObject == null || string.IsNullOrEmpty(methodName)) return;

            object instance = targetObject;
            var classType = AnimGraphClipInfo.FindTypeByName(targetName);

            if (classType != null && !isGameObject)
            {
                //Type classType = Type.GetType(type.FullName);
                if (typeof(MonoBehaviour).IsAssignableFrom(classType))
                {
                    var tempObj = new GameObject();
                    instance = tempObj.AddComponent(classType);
                    GameObject.DestroyImmediate(tempObj);
                }
                else
                    instance = Activator.CreateInstance(classType);
            }

            MethodInfo methodInfo = instance.GetType().GetMethod(methodName);

            if (methodInfo != null)
            {
                try
                {
                    List<object> parms = new List<object>();
                    parms.Add(g);
                    foreach (var p in parametersList)
                    {
                        parms.Add(p);
                    }
                    methodInfo.Invoke(instance, parms.ToArray());
                }
                catch (Exception)
                {
                    // Handle or log exception as needed
                }
            }
            else
            {
                Debug.Log("Method not found.");
            }
        }
    }

    public static class TypeCache
    {
        public static Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();
        public static Type FindTypeByName(string className)
        {
            // Check if type is already cached
            if (_cachedTypes.TryGetValue(className, out Type cachedType))
            {
                return cachedType;
            }

            // Search through all assemblies
            Type foundType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly =>
                {
                // Try to find type by exact name match
                var type = assembly.GetTypes()
                        .FirstOrDefault(t => t.Name == className);

                // If not found, try full name match
                return type ?? assembly.GetType(className);
                })
                .FirstOrDefault(type => type != null);

            // Cache the result if found
            if (foundType != null)
            {
                _cachedTypes[className] = foundType;
            }
            else
            {
                Debug.LogWarning($"Type '{className}' not found in any loaded assembly.");
            }

            return foundType;
        }
    }
}