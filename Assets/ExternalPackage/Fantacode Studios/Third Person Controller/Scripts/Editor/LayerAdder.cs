using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FS_ThirdPerson
{
    public class LayerAdder
    {
        //    public static string layerAddedKey = "Layer is not added";

        //    static string[] newLayers = { "FootTrigger" };


        //    [InitializeOnLoadMethod]
        //    public static void ShowWindow()
        //    {
        //        if (PlayerPrefs.GetString(layerAddedKey) != "Layer is added")
        //        {
        //            AddNewLayers();
        //            PlayerPrefs.SetString(layerAddedKey, "Layer is added");
        //        }
        //    }


        //    public static void AddNewLayers()
        //    {
        //        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        //        SerializedProperty layersProp = tagManager.FindProperty("layers");

        //        for (int i = 0; i < newLayers.Length; i++)
        //        {
        //            string layerName = newLayers[i];

        //            if (!string.IsNullOrEmpty(layerName))
        //            {
        //                bool layerExists = false;
        //                for (int j = 0; j < layersProp.arraySize; j++)
        //                {
        //                    SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(j);
        //                    if (layerProp.stringValue == layerName)
        //                    {
        //                        layerExists = true;
        //                        break;
        //                    }
        //                }

        //                if (!layerExists)
        //                {
        //                    for (int j = 8; j < layersProp.arraySize; j++)
        //                    {
        //                        SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(j);
        //                        if (string.IsNullOrEmpty(layerProp.stringValue))
        //                        {
        //                            layerProp.stringValue = layerName;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        tagManager.ApplyModifiedProperties();
        //    }
    }
}
