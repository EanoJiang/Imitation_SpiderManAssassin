using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace FS_ThirdPerson
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CustomIconAttribute : Attribute
    {
        public string IconPath { get; private set; }

        public CustomIconAttribute(string iconPath)
        {
            IconPath = iconPath;
        }
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
    public class CustomIconSetter
    {
        static CustomIconSetter()
        {
            EditorApplication.projectWindowItemOnGUI += SetCustomIcon;
        }

        static void SetCustomIcon(string guid, Rect selectionRect)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

            if (obj == null) return;

            Type objectType = obj.GetType();
            CustomIconAttribute iconAttribute = Attribute.GetCustomAttribute(objectType, typeof(CustomIconAttribute)) as CustomIconAttribute;

            if (iconAttribute != null)
            {
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconAttribute.IconPath);
                if (icon != null && EditorGUIUtility.GetIconForObject(obj) != icon)
                {
                    EditorGUIUtility.SetIconForObject(obj, icon);
                }
                else if (icon == null)
                {
                    //Debug.LogError($"Custom icon not found at the specified path: {iconAttribute.IconPath} for {objectType.Name}");
                }
            }
        }
    }
#endif
}