using FS_ThirdPerson;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace FS_ParkourSystem
{
    public class WelcomeWindow : EditorWindow
    {
        public static WelcomeWindow welcomeWindow;
        public const string inputsystem = "inputsystem";
        public const string invector = "invector";
        public const string gameCreator2 = "gameCreator2";

        public static string windowShowedKey = "FC_notShowed";

        static string[] newLayers = { "Obstacles", "Ledge" };


        [MenuItem("Tools/Parkour && Climbing System/Support/Discord")]
        public static void InitDiscord()
        {
            Application.OpenURL("https://discord.gg/QNe4AMYT");
        }
        [MenuItem("Tools/Parkour && Climbing System/Support/Youtube")]
        public static void InitYoutude()
        {
            Application.OpenURL("https://www.youtube.com/playlist?list=PLnbdyws4rcAu8b3zKIyTMKhMK8iHsIaOn");
        }


        [InitializeOnLoadMethod]
        public static void ShowWindow()
        {
            if (PlayerPrefs.GetString(windowShowedKey) != "FC_showed")
            {
                AddNewLayers();
                InitEditorWindow();
                PlayerPrefs.SetString(windowShowedKey, "FC_showed");
            }
        }
        [MenuItem("Tools/Parkour && Climbing System/Welcome Window")] 
        public static void InitEditorWindow()
        {
            if (HasOpenInstances<WelcomeWindow>())   
                return; 
            welcomeWindow = (WelcomeWindow)EditorWindow.GetWindow<WelcomeWindow>();
            GUIContent titleContent = new GUIContent("Welcome");
            welcomeWindow.titleContent = titleContent;
            welcomeWindow.minSize = new Vector2(450, 290);
            welcomeWindow.maxSize = new Vector2(450, 290);
            
        }
        private void OnGUI()
        {
            
            if (welcomeWindow == null)
                welcomeWindow = (WelcomeWindow)EditorWindow.GetWindow<WelcomeWindow>();
            GUILayout.Space(10);

            EditorGUI.HelpBox(new Rect(5, 10, position.width - 10, 80), "Parkour and Climbing system allows the player to traverse complex environments in the game using different parkour and climbing actions. The parkour system has predictive jumping that will automatically detect points to which the player can jump and execute precise jumps to reach them. The climbing system uses a mix of authored and procedural animations to adapt to dynamic climbing environments while looking realistic. ", MessageType.None);


            if (GUI.Button(new Rect(55, 100, 110, 35), "QuickStart"))
                Application.OpenURL("https://fantacode.gitbook.io/parkour-and-climbing-system/quickstart");
            if (GUI.Button(new Rect(170, 100, 110, 35), "Documentation"))
                Application.OpenURL("https://fantacode.gitbook.io/parkour-and-climbing-system/");
            if (GUI.Button(new Rect(285, 100, 110, 35), "Videos"))
                Application.OpenURL("https://www.youtube.com/playlist?list=PLnbdyws4rcAu8b3zKIyTMKhMK8iHsIaOn");

            GUILayout.Space(130);
            AddOnModules();

            GUI.Box(new Rect(0, 230, position.width, 2), "");

            if (GUI.Button(new Rect(155, 243, 150, 35), "Create Character"))
                ClimbPointAndCreateCharacterEditorWindow.InitPlayerSetupWindow();

        }
        private void AddOnModules()
        {
            var _inputsystem = false;
            var _invector = false;
            var _gameCreator2 = false;

            var sybmols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
            for (int i = 0; i < sybmols.Length; i++)
            {
                if (string.Equals(inputsystem, sybmols[i].Trim()))
                    _inputsystem = true;
                if (string.Equals(invector, sybmols[i].Trim()))
                    _invector = true;
                if (string.Equals(gameCreator2, sybmols[i].Trim()))
                    _gameCreator2 = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("Addon Module :"), EditorStyles.boldLabel);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            var _input = EditorGUILayout.Toggle("", _inputsystem, GUILayout.Width(17), GUILayout.Height(17));
            EditorGUILayout.LabelField(new GUIContent("New Input System", "Enabling this feature allows support for the New Input System. Ensure that you have installed the New InputSystem package before enabling this feature"), GUILayout.Width(110));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            var _invec = EditorGUILayout.Toggle("", _invector, GUILayout.Width(17), GUILayout.Height(17));
            EditorGUILayout.LabelField(new GUIContent("Invector Integration", "Enabling this feature allows integration with Invector's controllers. Ensure that you have installed the Invector's package before enabling this feature"), GUILayout.Width(130));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            var _gameC = EditorGUILayout.Toggle("", _gameCreator2, GUILayout.Width(17), GUILayout.Height(17));
            EditorGUILayout.LabelField(new GUIContent("GameCreator2 Integration", "Enabling this feature allows integration with GameCreator's controllers. Ensure that you have installed the GameCreator's package before enabling this feature."), GUILayout.Width(160));
            GUILayout.EndHorizontal();

            var sybmolValueChanged = EditorGUI.EndChangeCheck();

            if (_input != _inputsystem)
            {
                if (_input)
                {
                    if (EditorUtility.DisplayDialog("New Input System", "Enabling this feature allows support for the New Input System. Ensure that you have installed the New InputSystem package before enabling this feature", "OK", "Cancel"))
                    {
                        ScriptingDefineSymbolController.ToggleScriptingDefineSymbol(inputsystem, _input);
                    }
                    else
                        sybmolValueChanged = false;
                }
                else
                    ScriptingDefineSymbolController.ToggleScriptingDefineSymbol(inputsystem, _input);
            }

            if (_invec != _invector)
            {
                if (_invec)
                {
                    if (EditorUtility.DisplayDialog("Invector Integration", "Enabling this feature allows integration with Invector's controllers. Ensure that you have installed the Invector's package before enabling this feature", "OK", "Cancel"))
                    {
                        ScriptingDefineSymbolController.ToggleScriptingDefineSymbol(invector, _invec);
                    }
                    else
                        sybmolValueChanged = false;
                }
                else
                    ScriptingDefineSymbolController.ToggleScriptingDefineSymbol(invector, _invec);
            }

            if (_gameC != _gameCreator2)
            {
                if (_gameC)
                {
                    if (EditorUtility.DisplayDialog("Invector Integration", "Enabling this feature allows integration with GameCreator's controllers. Ensure that you have installed the GameCreator's package before enabling this feature.", "OK", "Cancel"))
                    {
                        ScriptingDefineSymbolController.ToggleScriptingDefineSymbol(gameCreator2, _gameC);
                    }
                    else
                        sybmolValueChanged = false;
                }
                else
                    ScriptingDefineSymbolController.ToggleScriptingDefineSymbol(gameCreator2, _gameC);
            }

            if (sybmolValueChanged)
                ScriptingDefineSymbolController.ReimportScripts();

        }
        [MenuItem("Tools/Parkour && Climbing System/Import tags and layers", false, 600, priority = 4)]
        public static void AddTagsAndlayers()
        {
            EditorUtility.DisplayDialog("tags and layers", "Tags and layers imported successfully", "ok");
            AddNewLayers();
        }
        public static void AddNewLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            for (int i = 0; i < newLayers.Length; i++)
            {
                string layerName = newLayers[i];

                if (!string.IsNullOrEmpty(layerName))
                {
                    bool layerExists = false;
                    for (int j = 0; j < layersProp.arraySize; j++)
                    {
                        SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(j);
                        if (layerProp.stringValue == layerName)
                        {
                            layerExists = true;
                            break;
                        }
                    }

                    if (!layerExists)
                    {
                        for (int j = 8; j < layersProp.arraySize; j++)
                        {
                            SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(j);
                            if (string.IsNullOrEmpty(layerProp.stringValue))
                            {
                                layerProp.stringValue = layerName;
                                break;
                            }
                        }
                    }
                }
            }
            tagManager.ApplyModifiedProperties();
            if (!InternalEditorUtility.tags.ToList().Contains("NarrowBeam"))
                InternalEditorUtility.AddTag("NarrowBeam");
            if (!InternalEditorUtility.tags.ToList().Contains("SwingableLedge"))
                InternalEditorUtility.AddTag("SwingableLedge");
        }
    }
}
