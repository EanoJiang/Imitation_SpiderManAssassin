using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.Utilities
{
    [CustomEditor(typeof(ReboneMesh))]
    public class ReboneMeshEd : Editor
    {
        ReboneMesh M;

        private void OnEnable()
        {
            M = (ReboneMesh)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginDisabledGroup(M.defaultMesh == null || M.newMesh == null);

            if (GUILayout.Button("Transfer Bones"))
            {
                M.TransferBones();
                EditorUtility.SetDirty(M);
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}