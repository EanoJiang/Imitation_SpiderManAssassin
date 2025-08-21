#if UNITY_EDITOR
using FS_ThirdPerson;
using UnityEditor;
using UnityEngine;
namespace FS_GrapplingSystem
{
    public class CreateGrapplingHookCharacterWindow : EditorWindow
    {
        public static CreateGrapplingHookCharacterWindow window;

        public GameObject model;
        bool isHumanoid;
        bool validAvathar;
        bool hasAnimator;
        bool validModel;



#if UNITY_EDITOR

        [MenuItem("Tools/Grappling Hook/Create Character", false, 2)]
        public static void InitPlayerSetupWindow()
        {
            window = GetWindow<CreateGrapplingHookCharacterWindow>();
            window.titleContent = new GUIContent("Grappling Hook");
        }


        private void OnGUI()
        {
            GetWindow();
            GUILayout.Space(10);
            SetWarningAndErrors();
            model = (GameObject)UndoField(model, EditorGUILayout.ObjectField("Character Model", model, typeof(GameObject), true));
            GUILayout.Space(2f);
            if (GUILayout.Button("Create Character"))
            {
                CreatePlayer();
            }
        }


        void CreatePlayer()
        {
            if (validModel)
            {
                var playerPrefab = (GameObject)Resources.Load("Grappling Controller");
                var grapplingController = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

                var model = Instantiate(this.model, Vector3.zero, Quaternion.identity);

                var playerController = grapplingController.GetComponentInChildren<PlayerController>();
                var playerGameObject = playerController.gameObject;
                var animator = playerGameObject.GetComponent<Animator>();
                var modelAnimator = model.GetComponent<Animator>();


                model.transform.SetParent(playerGameObject.transform);
                grapplingController.GetComponentInChildren<CameraController>().followTarget = model.transform;
                animator.avatar = modelAnimator.avatar;
                grapplingController.name = playerPrefab.name;
                model.name = this.model.name;

                var footTriggerPrefab = (GameObject)Resources.Load("FootTrigger");
                var rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot).transform;
                var leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot).transform;
                var rightCollider = PrefabUtility.InstantiatePrefab(footTriggerPrefab, rightFoot) as GameObject;
                var leftCollider = PrefabUtility.InstantiatePrefab(footTriggerPrefab, leftFoot) as GameObject;
                rightCollider.transform.localPosition = Vector3.zero;
                leftCollider.transform.localPosition = Vector3.zero;

                if ((rightCollider.layer != LayerMask.NameToLayer("FootTrigger")))
                    rightCollider.layer = LayerMask.NameToLayer("FootTrigger");
                if ((leftCollider.layer != LayerMask.NameToLayer("FootTrigger")))
                    leftCollider.layer = LayerMask.NameToLayer("FootTrigger");

                Undo.RegisterCreatedObjectUndo(grapplingController, "new character controller added");
                Undo.RegisterCreatedObjectUndo(model, "new character added");
                Selection.activeObject = grapplingController;
                SceneView sceneView = SceneView.lastActiveSceneView;
                sceneView.Focus();
                sceneView.LookAt(grapplingController.transform.position);
            }
        }

        void SetWarningAndErrors()
        {
            validModel = false;
            if (model != null)
            {
                var animator = model.GetComponent<Animator>();
                if (animator != null)
                {
                    hasAnimator = true;
                    isHumanoid = animator.isHuman;
                    validAvathar = animator.avatar != null && animator.avatar.isValid;
                }
                else
                    hasAnimator = isHumanoid = validAvathar = false;
                if (!hasAnimator)
                    EditorGUILayout.HelpBox("Animator Component is Missing", MessageType.Error);
                else if (!isHumanoid)
                    EditorGUILayout.HelpBox("Set your model animtion type to Humanoid", MessageType.Error);
                else if (!validAvathar)
                    EditorGUILayout.HelpBox(model.name + " is a invalid Humanoid", MessageType.Info);
                else
                {
                    EditorGUILayout.HelpBox("Make sure your FBX model is Humanoid", MessageType.Info);
                    validModel = true;
                }
                SetWindowHeight(95);
            }
            else
                SetWindowHeight(55);
        }
        static void SetWindowHeight(float height)
        {
            window.minSize = new Vector2(400, height);
            window.maxSize = new Vector2(400, height);
        }
        static void GetWindow()
        {
            if (window == null)
            {
                window = GetWindow<CreateGrapplingHookCharacterWindow>();
                window.titleContent = new GUIContent("Grappling Hook");
                SetWindowHeight(55);
            }
        }
        object UndoField(object oldValue, object newValue)
        {
            if (newValue != null && oldValue != null && newValue.ToString() != oldValue.ToString())
            {
                Undo.RegisterCompleteObjectUndo(this, "Update Field");
            }
            return newValue;
        }
#endif
    }
}
#endif
