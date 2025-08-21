using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using System.Collections.Generic;
using FS_ThirdPerson;

namespace FS_ParkourSystem
{
    public class ClimbPointAndCreateCharacterEditorWindow : EditorWindow
    {
        public static ClimbPointAndCreateCharacterEditorWindow window;
        [SerializeField] float distanceBetweenPoints = .75f;
        [SerializeField] bool bothSide;
        [SerializeField] static int selectedTab;

        public GameObject model;
        bool isHumanoid;
        bool validAvathar;
        bool hasAnimator;
        bool validModel;



#if UNITY_EDITOR
        private void OnEnable()
        {
            Undo.undoRedoPerformed += () => { Repaint(); TabSetup(); };
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= () => { Repaint(); TabSetup(); };
        }

        [MenuItem("Tools/Parkour && Climbing System/ClimbPoint Editor", false, 1)]
        public static void InitClimbPointEditorWindow()
        {
            window = GetWindow<ClimbPointAndCreateCharacterEditorWindow>();
            window.titleContent = new GUIContent("Climb Point");
            selectedTab = 0;
            TabSetup();
        }

        [MenuItem("Tools/Parkour && Climbing System/Create Character", false, 2)]
        public static void InitPlayerSetupWindow()
        {
            window = GetWindow<ClimbPointAndCreateCharacterEditorWindow>();
            window.titleContent = new GUIContent("Character");
            selectedTab = 1;
            TabSetup();
        }


        private async void OnGUI()
        {
            GetWindow();
            var curTab = selectedTab;
            selectedTab = (int)UndoField(selectedTab, GUILayout.Toolbar(selectedTab, new string[] { "Point Placer", "Create Character" }));
            GUILayout.Space(15);
            if (selectedTab == 0)
            {
                bothSide = (bool)UndoField(bothSide, EditorGUILayout.Toggle(new GUIContent("Both Side", "With this option, you can bake points on both sides of the ledge"), bothSide));

                distanceBetweenPoints = (float)UndoField(distanceBetweenPoints, EditorGUILayout.FloatField(new GUIContent("Distance Between Points", "Minimum distance is 0.05"), distanceBetweenPoints));

                using (var scope = new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Bake"))
                    {
                        var objs = Selection.gameObjects.ToList();

                        foreach (var obj in objs)
                        {
                            if (obj.GetComponent<MeshRenderer>() == null || distanceBetweenPoints <= 0)
                                continue;

                            obj.layer = LayerMask.NameToLayer("Ledge");


                            ClimbingPointUtils.totalPointCount = 0;
                            var points = obj.GetComponentsInChildren<ClimbPoint>().ToList();
                            for (int i = 0; i < points.Count; i++)
                            {
                                if (points[i].gameObject != obj)
                                {
                                    EditorUtility.DisplayProgressBar("Creating Points", $"Creating climbpoints for ledge - {obj.name}", ((float)i / points.Count) * .5f);
                                    if (ClimbingPointUtils.totalPointCount == 1000)
                                    {
                                        await Task.Delay((int)(Time.deltaTime * 1000.0f));
                                        ClimbingPointUtils.totalPointCount = 0;
                                    }
                                    Undo.DestroyObjectImmediate(points[i].gameObject);
                                    ClimbingPointUtils.totalPointCount++;
                                }
                            }

                            ClimbingPointUtils.totalPointCount = 0;
                            ClimbingPointUtils.BakePoints(obj, distanceBetweenPoints, bothSide, points.Count == 0 ? 0f : .5f);
                        }
                    }

                    else if (GUILayout.Button("Clear Points"))
                    {
                        var objs = Selection.gameObjects.ToList();
                        ClimbingPointUtils.ClearPoints(objs);
                    }
                }
            }
            else if (selectedTab == 1)
            {
                SetWarningAndErrors();
                model = (GameObject)UndoField(model, EditorGUILayout.ObjectField("Player Model", model, typeof(GameObject), true));
                GUILayout.Space(2f);
                if (GUILayout.Button("Create Character"))
                    CreateCharacter();
            }

            if (curTab != selectedTab)
            {
                TabSetup();
            }
        }


        void CreateCharacter()
        {
            if (validModel)
            {
                var playerPrefab = (GameObject)Resources.Load("Parkour Controller");
                var footTriggerPrefab = (GameObject)Resources.Load("FootTrigger");
                var parkourController = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                var model = Instantiate(this.model, Vector3.zero, Quaternion.identity);
                var player = parkourController.GetComponentInChildren<LocomotionController>().gameObject;
                model.transform.SetParent(player.transform);
                parkourController.GetComponentInChildren<CameraController>().followTarget = model.transform;

                var animator = player.GetComponent<Animator>();
                animator.avatar = this.model.GetComponent<Animator>().avatar;
                parkourController.name = playerPrefab.name;
                model.name = this.model.name;

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

                Undo.RegisterCreatedObjectUndo(parkourController, "new character controller added");
                Undo.RegisterCreatedObjectUndo(model, "new character added");
                Selection.activeObject = parkourController;
                SceneView sceneView = SceneView.lastActiveSceneView;
                sceneView.Focus();
                sceneView.LookAt(parkourController.transform.position);
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
                SetWindowHeight(123);
            }
            else
                SetWindowHeight(83);
        }
        static void TabSetup()
        {
            string title = "";
            if (selectedTab == 0)
            {
                SetWindowHeight(101);
                title = "Climb Point";
            }
            else if (selectedTab == 1)
            {
                SetWindowHeight(123);
                title = "Character";
            }

            window.titleContent = new GUIContent(title);
        }
        static void SetWindowHeight(float height)
        {
            window.minSize = new Vector2(400, height);
            window.maxSize = new Vector2(400, height);
        }
        void GetWindow()
        {
            if (window == null)
            {
                window = GetWindow<ClimbPointAndCreateCharacterEditorWindow>();
                window.titleContent = new GUIContent("Editor");
                TabSetup();
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