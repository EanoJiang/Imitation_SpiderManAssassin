#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FS_ThirdPerson
{
    public class FSSystemsSetupEditorWindow : EditorWindow
    {

        private static bool isEditorLoading = true;

        [InitializeOnLoadMethod]
        public static void OnProjectLoad()
        {
            if (!SessionState.GetBool("FS_WelcomeWindow_Loaded", false)) 
            {
                SessionState.SetBool("FS_WelcomeWindow_Loaded", true);
                EditorApplication.update += OnFirstUpdate; 
            }
        }

        private static void OnFirstUpdate()
        {
            if (!isEditorLoading)
                return;

            if (EditorWindow.focusedWindow != null)
            {
                isEditorLoading = false; 
                EditorApplication.update -= OnFirstUpdate;
                CreateSetupScript();
                setupScript.ImportProjectSettings();
                ShowWindow();
                setupScript.FindSystem();
            }
        }



        static FSSystemsSetup setupScript;
        public Vector2 scrolPosition = Vector2.zero;

        private float dividerPosition = 150f;

        private int selectedTab = 0;
        private string[] toolbarOptions = { "Welcome", "Setup", "Packages" };

        static Rect tabRect;
        static Rect windowRect;

        static FSSystemsSetupEditorWindow window;


        [MenuItem("Tools/FS Systems")]
        public static void ShowWindow()
        {
            if (!HasOpenInstances<FSSystemsSetupEditorWindow>())
            {
                window = GetWindow<FSSystemsSetupEditorWindow>("FS Systems");
                SetWindowHeight(672);
                CreateSetupScript();
                setupScript.FindSystem();
            }
        }

        private void OnEnable()
        {
            setupScript = FindObjectOfType<FSSystemsSetup>();
        }

        private void OnDisable()
        {
            if (setupScript != null)
                GameObject.DestroyImmediate(setupScript.gameObject);
        }

        private void OnDestroy()
        {
            if (setupScript != null)
                GameObject.DestroyImmediate(setupScript.gameObject);
        }

        private void OnGUI()
        {
            DrawTitle();
            DrawSlider();
            DrawTabs();



            if (!Event.current.Equals(EventType.Repaint))
                Repaint();
        }

        #region Window

        private void DrawTitle()
        {
            GUI.backgroundColor = Color.blue;
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.Space(10);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 25,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.031f, 0.506f, 0.992f) },
                hover = { textColor = new Color(0.031f, 0.506f, 0.992f) }
            };
            GUILayout.Label("Fantacode Studios", titleStyle);

            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }
        private void DrawSlider()
        {
            var y = 59;
            tabRect = new Rect(0, y, dividerPosition, position.height - y);
            windowRect = new Rect(tabRect.width, y, position.width - tabRect.width, position.height - y);
            GUI.Box(tabRect, "");
        }
        private void DrawTabs()
        {
            var tRect = tabRect;
            tRect.x = 2;
            GUILayout.BeginArea(tRect);

            for (int i = 0; i < toolbarOptions.Length; i++)
            {
                GUI.backgroundColor = i == selectedTab ? Color.cyan : Color.white;

                if (GUILayout.Button(toolbarOptions[i], GUILayout.Height(25), GUILayout.Width(tRect.width - 4)))
                {
                    SetWindowHeight(672);
                    selectedTab = i;
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndArea();

            GUILayout.BeginArea(windowRect);

            GUILayout.BeginVertical(EditorStyles.helpBox);


            CreateSetupScript();
            switch (selectedTab)
            {
                case 0:
                    WelcomeWindow();
                    break;
                case 1:
                    SetupWindow();
                    break;
                case 2:
                    PackageWindow();
                    break;
            }
            GUILayout.EndVertical();


            GUILayout.EndArea();
        }

        #endregion

        #region welcome

        void WelcomeWindow()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Welcome to Fantacode Studios!");

            if (GUILayout.Button("Import Project Settings"))
            {
                setupScript.ImportProjectSettings();
            }

            GUILayout.EndHorizontal();

            var iconStyle = new GUIStyle()
            {
                fixedHeight = 500,
                fixedWidth = windowRect.width - 8,
                padding = { left = 10, right = 10, top = 10,bottom = 10 }

            };
            if (welcomeWindowImg == null)
                welcomeWindowImg = Resources.Load<Texture2D>("Icons/Main_img");

            GUILayout.Label(welcomeWindowImg, iconStyle);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); 

            DrawSocialMedia("Icons/youtube_icon", "Fanatcode Studios", ref youtubeIcon, "https://youtube.com/@fantacodestudios?si=zUXiCBf0tqW1vcGo");
            DrawSocialMedia("Icons/discord_icon", "Fanatcode Studios", ref discordicon, "https://discord.com/invite/9UP9uH6mKZ");
            DrawSocialMedia("Icons/youtube_icon", "GameDev Experiments", ref youtubeIcon, "https://youtube.com/@gamedevexperiments?si=kZVaykdzCbxpDbBx");

            GUILayout.FlexibleSpace(); 
            GUILayout.EndHorizontal();


        }

        private void DrawSocialMedia(string iconPath, string label, ref Texture2D icon, string link)
        {

            GUILayout.BeginVertical();
            if (icon == null)
                icon = Resources.Load<Texture2D>(iconPath);


            GUILayout.BeginHorizontal();
            GUILayout.Space(windowRect.width / 6 - 20);
            if (GUILayout.Button(new GUIContent("", icon), GUILayout.Width(40), GUILayout.Height(40)))
            {
                Application.OpenURL(link);
            }
            GUILayout.EndHorizontal();


            var fsYoutubeTextLableStyle = new GUIStyle()
            {
                fixedHeight = 35,
                fixedWidth = windowRect.width / 3,
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold
            };
            GUILayout.Label(label, fsYoutubeTextLableStyle);
            GUILayout.EndVertical();
        }

        #endregion

        #region Setup

        Editor characterPreview;
        public GameObject characterModel;
        GameObject currentPreviewObj;
        string animatorName = "Combined Controller";


        private void SetupWindow()
        {
            GUI.backgroundColor = Color.black;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.3f, 0.6f, 0.8f) },
                hover = { textColor = new Color(0.3f, 0.6f, 0.8f) }
            };
            GUILayout.Space(10);
            GUILayout.Label("FS Systems Setup", headerStyle);
            GUILayout.Space(10);
            GUI.backgroundColor = Color.white;
            GUIStyle findButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.BoldAndItalic,
                fixedHeight = 20,
                fixedWidth = 150,
                normal = { textColor = Color.black },
                hover = { textColor = Color.black }
            };

            Rect buttonRect = new Rect(windowRect.width - 158, 35, 150, 20);
            //GUI.backgroundColor = new Color(0.2f, 0.4f, 0.8f, 0.8f);
            if (setupScript.FSSystems.Count == 0 || GUI.Button(buttonRect, "Load Systems", findButtonStyle))
            {
                setupScript.FindSystem();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(10);
            GUILayout.EndVertical();
            var systems = setupScript.FSSystems.Where(s => s.Value.isSystemBase).ToList();
            scrolPosition = GUILayout.BeginScrollView(scrolPosition, GUILayout.Height(Mathf.Min(systems.Count * 40, 200)));

            foreach (var item in systems)
            {
                DrawFancyToggle(item.Value.name, ref item.Value.enabled);
            }

            GUILayout.EndScrollView();
            GUILayout.Space(10);
            SetWarningAndErrors();
            characterModel = (GameObject)EditorGUILayout.ObjectField("Character Model", characterModel, typeof(GameObject), true);

            GUILayout.Space(2);
            GUIContent textFieldLabel = new GUIContent("Animator Name", "The name for the new Animator Controller being created.");
            animatorName = EditorGUILayout.TextField(textFieldLabel, animatorName);

            if (characterModel != null)
            {
                if (characterPreview == null || currentPreviewObj != characterModel)
                {
                    characterPreview = Editor.CreateEditor(characterModel);
                    currentPreviewObj = characterModel;
                }

                characterPreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(150, 250), EditorStyles.whiteLabel);
            }

            // Button Container
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            // Reset Button 
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 30,
                normal = { textColor = Color.white },
                hover = { textColor = Color.yellow }
            };

            GUI.backgroundColor = new Color(0.6f, 0.2f, 0.2f, 0.8f); // Soft Red
            if (GUILayout.Button("Reset", buttonStyle, GUILayout.Width(position.width * 0.3f)))
            {
                HandleReset();
            }

            GUILayout.Space(10);


            GUI.backgroundColor = new Color(0.2f, 0.6f, 0.2f, 0.8f); // Soft Green
            if (GUILayout.Button("Setup", buttonStyle, GUILayout.ExpandWidth(true)))
            {
                HandleReset();
                HandleSetup();
            }


            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

        }

        private void DrawFancyToggle(string label, ref bool value)
        {
            Rect rect = GUILayoutUtility.GetRect(200, 40, GUILayout.ExpandWidth(true));
            bool isMouseOver = rect.Contains(Event.current.mousePosition);

            Color bgColor = value ? new Color(0.3f, 0.6f, 0.3f, 0.4f) : new Color(0.6f, 0.3f, 0.3f, 0.4f);
            Color hoverColor = new Color(1f, 1f, 1f, 0.1f);
            EditorGUI.DrawRect(rect, bgColor);
            if (isMouseOver)
            {
                EditorGUI.DrawRect(rect, hoverColor);
            }

            // Toggle box
            Rect toggleRect = new Rect(rect.x + 10, rect.y + 10, 20, 20);
            value = GUI.Toggle(toggleRect, value, GUIContent.none);

            // Label
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.9f, 0.9f, 0.9f) } // Light Gray
            };
            GUI.Label(new Rect(rect.x + 40, rect.y + 10, rect.width - 50, rect.height - 20), label, labelStyle);

            if (Event.current.type == EventType.MouseDown && isMouseOver)
            {

                value = !value;
                Event.current.Use();
            }
        }

        private void HandleSetup()
        {
            if (!validForCreateCharacter || setupScript == null) return;

            string sourcePath = AssetDatabase.GetAssetPath(Resources.Load("AnimatorControllers/Third Person Controller"));
            string destinationPath = $"{Path.GetDirectoryName(sourcePath)}/{animatorName}.controller";
            //string destinationPath = "Assets/Combined Controller.controller";
            AssetDatabase.CopyAsset(sourcePath, destinationPath);
            AssetDatabase.Refresh();

            AnimatorController CombinedController = AssetDatabase.LoadAssetAtPath<AnimatorController>(destinationPath);
            AnimatorMergerUtility animatorMergerUtility = new AnimatorMergerUtility(CombinedController, CombinedController);


            (var player, var model, var systemControllerParentObj) = SetCharacterModelAsChild();
            foreach (var item in setupScript.FSSystems)
            {
                if (item.Value.enabled)
                {
                    var systemPrefabObj = setupScript.CopyComponentsAndAnimControllerFromPrefab(item.Value.prefabName, animatorMergerUtility, player);
                    if (systemPrefabObj != null)
                        item.Value.setupExtraActions?.Invoke(model, systemPrefabObj, systemControllerParentObj);

                    if (!string.IsNullOrEmpty(item.Value.mobileControllerPrefabName))
                    {
                        var mobileControllerParent = systemControllerParentObj.transform.Find("Mobile Controller");
                        GameObject mcPrefab = Resources.Load<GameObject>(item.Value.mobileControllerPrefabName);
                        var mc = Instantiate(mcPrefab, mobileControllerParent);
                        mc.name = mcPrefab.name;
                        //mc.transform.SetParent(mobileControllerParent);
                    }
                }
            }
            Debug.Log($"A new Animator Controller named {animatorName} has been created and stored in the {destinationPath} directory.");


            if (characterModel)
            {
                player.GetComponent<Animator>().avatar = characterModel.GetComponent<Animator>().avatar;
            }
            player.GetComponent<Animator>().runtimeAnimatorController = CombinedController;

        }

        private void HandleReset()
        {
            if (setupScript == null) return;

            Component[] components = setupScript.GetComponents<Component>();

            foreach (Component component in components)
            {
                // Skip the component we want to keep and the Transform component
                if (component is FSSystemsSetup || component is Transform)
                    continue;

                // Destroy the component
                DestroyImmediate(component);
            }

            Transform parentTransform = setupScript.transform;

            for (int i = parentTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = parentTransform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }
        }

        static void CreateSetupScript()
        {
            if (setupScript == null)
            {
                GameObject go = new GameObject("FS System");
                setupScript = go.AddComponent<FSSystemsSetup>();
                setupScript.FSSystems = new Dictionary<string, FSSystemInfo>();

                setupScript.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        private (GameObject, GameObject, GameObject) SetCharacterModelAsChild()
        {
            GameObject prefab = Resources.Load<GameObject>("Locomotion Controller");
            GameObject instance = Instantiate(prefab);
            instance.name = "FS Player";

            var player = instance.GetComponentInChildren<PlayerController>().gameObject;
            var camera = instance.GetComponentInChildren<CameraController>();

            camera.followTarget = player.transform;
            GameObject model = Instantiate(characterModel, Vector3.zero, Quaternion.identity);
            model.transform.SetParent(player.transform);

            var animator = model.GetComponent<Animator>();
            AddFootTrigger(animator);


            if (player.layer != LayerMask.NameToLayer("Player"))
                player.layer = LayerMask.NameToLayer("Player");



            return (player, model, instance);
        }


        private void AddFootTrigger(Animator animator)
        {
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

        }

        #region warning

        bool isHumanoid;
        bool validAvathar;
        bool hasAnimator;
        bool systemsSelected;
        bool animatorNameIsValid;
        bool validForCreateCharacter;
        bool SetWarningAndErrors()
        {
            validForCreateCharacter = false;
            if (characterModel != null)
            {
                if (setupScript != null)
                    systemsSelected = setupScript.FSSystems.Where(s => s.Value.enabled).ToList().Count > 0;

                animatorNameIsValid = !string.IsNullOrEmpty(animatorName);

                var animator = characterModel.GetComponent<Animator>();
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
                    EditorGUILayout.HelpBox(characterModel.name + " is a invalid Humanoid", MessageType.Info);
                else if (!systemsSelected)
                    EditorGUILayout.HelpBox("Please select a system", MessageType.Error);
                else if (!animatorNameIsValid)
                    EditorGUILayout.HelpBox("Animator name is empty", MessageType.Error);
                else
                {
                    SetWindowHeight(672);
                    validForCreateCharacter = true;
                    return validForCreateCharacter;
                }
                SetWindowHeight(712);
            }
            else
            {
                SetWindowHeight(672);
                EditorGUILayout.HelpBox("Character model is not specified.", MessageType.Error);
            }

            return validForCreateCharacter;
        }

        #endregion

        #endregion

        #region Packages

        private void PackageWindow()
        {
            if (setupScript.FSSystems.Count == 0)
            {
                setupScript.FindSystem();
            }

            GUI.backgroundColor = Color.black;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.3f, 0.6f, 0.8f) },
                hover = { textColor = new Color(0.3f, 0.6f, 0.8f) }
            };
            GUILayout.Space(10);
            GUILayout.Label("FS Systems", headerStyle);
            GUILayout.Space(10);
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();


            foreach (var package in FSPackageData.packages)
            {
                GUILayout.Space(5);
                ShowPackages(package);
            }
        }

        Texture2D youtubeIcon;
        Texture2D documentIcon;
        Texture2D welcomeWindowImg;
        Texture2D discordicon;

        private void ShowPackages(FSPackageInfo info, float leftPadding = 0f)
        {
            Rect rect = GUILayoutUtility.GetRect(200, 40, GUILayout.ExpandWidth(true));
            rect.x += leftPadding;
            rect.width -= leftPadding;

            var iconRect = new Rect(rect.x + 5, rect.y + 5, rect.height - 10, rect.height - 10);
            var youtubeButtonRect = new Rect(rect.x + rect.width - 95, rect.y + 5, 30, 30);
            var docButtonRect = new Rect(youtubeButtonRect.x + 35, rect.y + 5, 30, 30);
            var storeButtonRect = new Rect(rect.x + rect.width - 25, rect.y + 5, 20, 30);
            bool isMouseOver = rect.Contains(Event.current.mousePosition) && !youtubeButtonRect.Contains(Event.current.mousePosition)
                && !docButtonRect.Contains(Event.current.mousePosition) && !storeButtonRect.Contains(Event.current.mousePosition);


            bool systemIsInstalled = false;
            foreach (var system in setupScript.FSSystems.Values)
            {
                if (system.name == info.packageName)
                {
                    systemIsInstalled = true;
                    break;
                }

            }

            Color bgColor = systemIsInstalled ? new Color(0.3f, 0.6f, 0.3f, 0.4f) : new Color(0.6f, 0.3f, 0.3f, 0.4f);
            Color hoverColor = new Color(1f, 1f, 1f, 0.1f);
            EditorGUI.DrawRect(rect, bgColor);
            if (isMouseOver)
            {
                EditorGUI.DrawRect(rect, hoverColor);
            }

            //if (Event.current.type == EventType.MouseDown && isMouseOver)
            //{
            //    FSPackageData.UnSelectAll();
            //    info.opened = !info.opened;
            //    Event.current.Use();
            //}



            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                padding = { left = (int)(iconRect.x + iconRect.width) + 5 - (int)leftPadding },
                normal = { textColor = new Color(0.9f, 0.9f, 0.9f) },
            };
            if (info.icon == null)
                info.icon = Resources.Load<Texture2D>(info.iconPath);
            GUI.Label(iconRect, info.icon);
            GUI.Label(rect, info.packageName, labelStyle);

           

            //GUIStyle installedTextstyle = new GUIStyle(EditorStyles.label)
            //{
            //    fontSize = 10,
            //    fontStyle = FontStyle.Italic,
            //    alignment = TextAnchor.MiddleCenter,
            //    normal = { textColor = systemIsInstalled ? Color.green : Color.red },
            //    hover = { textColor = systemIsInstalled ? Color.green : Color.red }
            //};
            //var r = new Rect(rect.width - 200, rect.y, 100, rect.height);
            //GUI.Label(r, systemIsInstalled?"Installed":"Not Installed", installedTextstyle);

            if (youtubeIcon == null)
                youtubeIcon = Resources.Load<Texture2D>("Icons/youtube_icon");

            if (documentIcon == null)
                documentIcon = Resources.Load<Texture2D>("Icons/document_icon");


            if (GUI.Button(youtubeButtonRect, new GUIContent("", youtubeIcon)))
            {
                Application.OpenURL(info.youtubeLink);
            }
            if (GUI.Button(docButtonRect, new GUIContent("", documentIcon)))
            {
                Application.OpenURL(info.documentationLink);
            }

            if (GUI.Button(storeButtonRect, new GUIContent(">")))
            {
                Application.OpenURL(info.assetLink);
            }

            foreach (var addOn in info.addOns)
            {
                ShowPackages(addOn, rect.x + 20);
            }
        }

        #endregion

        static void SetWindowHeight(float height)
        {
            if (window == null)
                window = GetWindow<FSSystemsSetupEditorWindow>("FS Systems");
            window.minSize = new Vector2(696, height);
            window.maxSize = new Vector2(696, height);
        }
    }
}
#endif