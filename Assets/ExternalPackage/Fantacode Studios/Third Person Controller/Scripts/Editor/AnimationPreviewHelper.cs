using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FS_ThirdPerson
{
    public static class AnimationPreviewHelper
    {
        private const BindingFlags PRIVATE_FIELD_BINDING_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
        private const BindingFlags PUBLIC_FIELD_BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;
        private const BindingFlags PUBLIC_PROPERTY_BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty;

        private static Type animationClipEditorType = Type.GetType("UnityEditor.AnimationClipEditor,UnityEditor");
        private static Type avatarPreviewType = Type.GetType("UnityEditor.AvatarPreview,UnityEditor");
        private static Type timeControlType = Type.GetType("UnityEditor.TimeControl,UnityEditor");
        private static Type propertyEditorType = Type.GetType("UnityEditor.PropertyEditor, UnityEditor");

        public static Object animationClipEditor;
        public static Object propertyEditor;
        public static MethodInfo closeMethod;
        public static PropertyInfo normalizedTimeProperty;
        public static PropertyInfo playingProperty;
        public static FieldInfo isEditorOpenField;
        public static object timeControl;

        public static bool repaint;
        static AnimationClip currentlPreviewClip;


        public async static void PlayPreview(AnimationClip clip)
        {
            propertyEditor = Resources.FindObjectsOfTypeAll(propertyEditorType).FirstOrDefault();
            await Task.Delay(100);
            animationClipEditor = Resources.FindObjectsOfTypeAll(animationClipEditorType).FirstOrDefault();
            SaveCurrentPreviewEditor();
            repaint = true;
            currentlPreviewClip = clip;
        }
        public static void ClosePreview()
        {
            if (propertyEditor != null)
            {
                if (closeMethod == null)
                    closeMethod = propertyEditorType.GetMethod("Close", BindingFlags.Public | BindingFlags.Instance);
                if (closeMethod != null && propertyEditor.GetType() == propertyEditorType)
                {
                    closeMethod.Invoke(propertyEditor, null);
                }
            }
            repaint = false;
        }

        public static void RepaintWindow()
        {
            if (animationClipEditor != null)
            {
                if (playingProperty == null || timeControl == null) return;

                var playing = (bool)playingProperty.GetValue(timeControl);
                if (playing)
                    ((Editor)animationClipEditor).Repaint();
            }
        }



        public static void SetAnimationTime(float time, AnimationClip clip)
        {
            if (clip == currentlPreviewClip && normalizedTimeProperty != null && timeControl != null && animationClipEditor != null)
            {
                normalizedTimeProperty.SetValue(timeControl, time);
                ((Editor)animationClipEditor).Repaint();
            }
        }

        static void SaveCurrentPreviewEditor()
        {
            if (animationClipEditor == null) return;

            var avatarPreview = animationClipEditorType.GetField("m_AvatarPreview", PRIVATE_FIELD_BINDING_FLAGS)?.GetValue(animationClipEditor);
            if (avatarPreview == null) return;

            timeControl = avatarPreviewType.GetField("timeControl", PUBLIC_FIELD_BINDING_FLAGS)?.GetValue(avatarPreview);

            var editorType = ((Editor)animationClipEditor).GetType();

            foreach (var item in animationClipEditorType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (item.Name == "isOpen")
                    Debug.Log(item.Name);
            }

            playingProperty = timeControlType.GetProperty("playing", PUBLIC_FIELD_BINDING_FLAGS);
            if (playingProperty != null && timeControl != null)
            {
                playingProperty.SetValue(timeControl, true);
            }

            normalizedTimeProperty = timeControlType.GetProperty("normalizedTime", PUBLIC_PROPERTY_BINDING_FLAGS);
        }

        private static bool wasWindowOpen = false;
        [InitializeOnLoadMethod]
        static void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += ClosePreview;
            EditorApplication.update += Update;
        }
        private static void Update()
        {
            if (repaint)
            {
                bool isWindowOpen = animationClipEditor != null && !animationClipEditor.Equals(null); 
                if (wasWindowOpen && !isWindowOpen)
                    repaint = false;
                wasWindowOpen = isWindowOpen;
                RepaintWindow();
            }
        }
    }
}