#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FS_ThirdPerson
{
    public partial class FSSystemsSetup
    {
        static FSSystemInfo GrapplingHookSystemSetup = new FSSystemInfo
        (
            isSystemBase: true,
            enabled: false,
            name: "Grappling Hook System",
            prefabName: "Grappling Controller",
            welcomeEditorShowKey: "GrapplingHookSystem_WelcomeWindow_Opened",
            mobileControllerPrefabName: "Grappling Hook Mobile Controller"
        );

        static string GrapplingHookSystemWelcomeEditorKey => GrapplingHookSystemSetup.welcomeEditorShowKey;


        [InitializeOnLoadMethod]
        public static void LoadGrapplingHookSystem()
        {
            if (!string.IsNullOrEmpty(GrapplingHookSystemWelcomeEditorKey) && !EditorPrefs.GetBool(GrapplingHookSystemWelcomeEditorKey, false))
            {
                SessionState.SetBool("FS_WelcomeWindow_Loaded", false);
                EditorPrefs.SetBool(GrapplingHookSystemWelcomeEditorKey, true);
                FSSystemsSetupEditorWindow.OnProjectLoad();
            }
        }
    }
}
#endif