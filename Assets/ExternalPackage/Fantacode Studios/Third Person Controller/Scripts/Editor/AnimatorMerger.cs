using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FS_ThirdPerson
{
    public class AnimatorMerger : EditorWindow
    {
        private AnimatorController sourceController;
        private AnimatorController targetController;

        //[MenuItem("Tools/Merge Animator Controllers")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(AnimatorMerger), false, "Animator Merger");
            window.minSize = new Vector2(400, 65);
            window.maxSize = new Vector2(400, 65);
        }

        void OnGUI()
        {
            //GUILayout.Label("Merge Animator Controllers", EditorStyles.boldLabel);

            sourceController = EditorGUILayout.ObjectField("Source Controller", sourceController, typeof(AnimatorController), false) as AnimatorController;
            targetController = EditorGUILayout.ObjectField("Target Controller", targetController, typeof(AnimatorController), false) as AnimatorController;

            if (GUILayout.Button("Merge"))
            {
                AnimatorMergerUtility animatorMergerUtiliy = new(sourceController, targetController);
                animatorMergerUtiliy.MergeAnimatorControllers(sourceController, targetController);
            }
        }
    }

   
}