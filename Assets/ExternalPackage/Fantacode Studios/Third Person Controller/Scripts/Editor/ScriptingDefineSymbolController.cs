using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FS_ThirdPerson
{
    public static class ScriptingDefineSymbolController
    {
        public static void ToggleScriptingDefineSymbol(string symbol, bool value)
        {
            if (value == true)
                AddingDefineSymbols(symbol);
            else
                RemovingDefineSymbols(symbol);
        }

        public static void AddingDefineSymbols(string symbol)
        {
            foreach (var group in GetInstalledBuildTargetGroups())
            {
                try
                {
                    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                    if (!string.IsNullOrEmpty(defines))
                        defines += ";";
                    defines += symbol;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            ReloadScript();
        }

        public static void RemovingDefineSymbols(string symbol)
        {
            foreach (var group in GetInstalledBuildTargetGroups())
            {
                try
                {
                    var symbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
                    symbols.Remove(symbol);
                    var defines = string.Join(";", symbols.ToArray());
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            ReloadScript();
        }

        public static HashSet<BuildTargetGroup> GetInstalledBuildTargetGroups()
        {
            var targetGroups = new HashSet<BuildTargetGroup>();
            foreach (BuildTarget target in (BuildTarget[])Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
                if (BuildPipeline.IsBuildTargetSupported(group, target))
                {
                    targetGroups.Add(group);
                }
            }
            return targetGroups;
        }

        public static void ReimportScripts()
        {
            AssetDatabase.ImportAsset("Assets/Fantacode Studios");
        }

        public static void ReloadScript()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.RequestScriptReload();
        }
    }
}