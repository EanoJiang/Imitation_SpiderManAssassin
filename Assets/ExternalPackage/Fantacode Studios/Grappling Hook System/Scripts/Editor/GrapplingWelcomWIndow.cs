#if UNITY_EDITOR
using FS_ThirdPerson;
using UnityEditor;
using UnityEngine;
namespace FS_GrapplingSystem
{
    public class GrapplingWelcomeWindow : EditorWindow
    {
        public static GrapplingWelcomeWindow combatCombatWelcomeWindow;
        public const string inputsystem = "inputsystem";
        public static string windowOpenKey = "grappling-window-not-opened";


        [MenuItem("Tools/Grappling Hook/Support/Discord")]
        public static void InitDiscord()
        {
            Application.OpenURL("https://discord.gg/QNe4AMYT");
        }
        [MenuItem("Tools/Grappling Hook/Support/Youtube")]
        public static void InitYoutude()
        {
            Application.OpenURL("https://youtube.com/playlist?list=PLnbdyws4rcAuJ6g8xrL9r2K3vv2SwL1na&si=ZEldtXG4X6XTpAsY");
        }


        [InitializeOnLoadMethod]
        public static void ShowWindow()
        {
            if (PlayerPrefs.GetString(windowOpenKey) != "grappling-window-opened")
            {
                InitEditorWindow();
                PlayerPrefs.SetString(windowOpenKey, "grappling-window-opened");
            }
        }
        [MenuItem("Tools/Grappling Hook/Welcome Window")]
        public static void InitEditorWindow()
        {
            if (HasOpenInstances<GrapplingWelcomeWindow>())
                return;
            combatCombatWelcomeWindow = (GrapplingWelcomeWindow)EditorWindow.GetWindow<GrapplingWelcomeWindow>();
            GUIContent titleContent = new GUIContent("Welcome");
            combatCombatWelcomeWindow.titleContent = titleContent;
            combatCombatWelcomeWindow.minSize = new Vector2(450, 242);
            combatCombatWelcomeWindow.maxSize = new Vector2(450, 242);

        }
        private void OnGUI()
        {
            if (combatCombatWelcomeWindow == null)
                combatCombatWelcomeWindow = (GrapplingWelcomeWindow)EditorWindow.GetWindow<GrapplingWelcomeWindow>();
            GUILayout.Space(10);

            EditorGUI.HelpBox(new Rect(5, 10, position.width - 10, 80), "Enhance your gameplay with an immersive grappling hook mechanic inspired by the Batman Arkham series! This system lets players traverse levels with ease, launching a hook to pull themselves or swing dynamically between points.", MessageType.None);


            if (GUI.Button(new Rect(55, 100, 110, 35), "QuickStart"))
                Application.OpenURL("https://fantacode.gitbook.io/grappling-hook-system/quick-start");
            if (GUI.Button(new Rect(170, 100, 110, 35), "Documentation"))
                Application.OpenURL("https://fantacode.gitbook.io/grappling-hook-system");
            if (GUI.Button(new Rect(285, 100, 110, 35), "Videos"))
                Application.OpenURL("https://www.youtube.com/playlist?list=PLnbdyws4rcAsY-QDOKJdTjBoTNZknwcWL");

            GUILayout.Space(130);
            AddOnModules();

            GUI.Box(new Rect(0, 182, position.width, 2), "");

            if (GUI.Button(new Rect(155, 195, 150, 35), "Create Character"))
                CreateGrapplingHookCharacterWindow.InitPlayerSetupWindow();

        }
        private void AddOnModules()
        {
            var _inputsystem = false;

            var sybmols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
            for (int i = 0; i < sybmols.Length; i++)
            {
                if (string.Equals(inputsystem, sybmols[i].Trim()))
                    _inputsystem = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("Addon Module :"), EditorStyles.boldLabel);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            var _input = EditorGUILayout.Toggle("", _inputsystem, GUILayout.Width(17), GUILayout.Height(17));
            EditorGUILayout.LabelField(new GUIContent("New Input System", "Enabling this feature allows support for the New Input System. Ensure that you have installed the New InputSystem package before enabling this feature"), GUILayout.Width(110));
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


            if (sybmolValueChanged)
                ScriptingDefineSymbolController.ReimportScripts();

        }
    }
}
#endif
