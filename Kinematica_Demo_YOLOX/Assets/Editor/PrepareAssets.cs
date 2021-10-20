using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrepareAssets : MonoBehaviour
{

    [MenuItem("Tools/OpenVINO/Copy to StreamingAssets")]
    static void CopyModels()
    {
        if (AssetDatabase.IsValidFolder("Assets/StreamingAssets") == false)
        {
            Debug.Log("Creating StreamingAssets folder.");
            AssetDatabase.CreateFolder("Assets", "StreamingAssets");
        }

        if (AssetDatabase.IsValidFolder("Assets/StreamingAssets/models") == false)
        {
            Debug.Log("Copying models folder to StreamingAssets folder.");
            bool success = AssetDatabase.CopyAsset("Assets/OpenVINO/models", "Assets/StreamingAssets/models");
            Debug.Log(success);
        }
        else
        {
            Debug.Log("models folder already exists in StreamingAssets folder");
        }

        AssetDatabase.CopyAsset("Assets/OpenVINO/Plugins/x86_64/plugins.xml", "Assets/StreamingAssets/plugins.xml");
    }

    //[MenuItem("Tools/OpenVINO/Copy to Plugins")]
    //static void CopyDLLs()
    //{
    //    if (AssetDatabase.IsValidFolder("Assets/Plugins") == false)
    //    {
    //        Debug.Log("Creating Plugins folder.");
    //        AssetDatabase.CreateFolder("Assets", "Plugins");

    //        //if (AssetDatabase.IsValidFolder("Assets/Plugins/x86_64") == false)
    //        //{
    //        //    Debug.Log("Creating x86_64 folder.");
    //        //    AssetDatabase.CreateFolder("Assets/Plugins", "x86_64");
    //        //}
    //    }

        

    //    if (AssetDatabase.IsValidFolder("Assets/Plugins/x86_64") == false)
    //    {
    //        Debug.Log("Copying DLLs to Plugins/x86_64 folder.");
    //        bool success = AssetDatabase.CopyAsset("Assets/OpenVINO/Plugins/x86_64", "Assets/Plugins/x86_64");
    //        Debug.Log(success);
    //        AssetDatabase.Refresh();
    //    }
    //    else
    //    {
    //        //Debug.Log("models folder already exists in StreamingAssets folder");
    //    }
    //}

    [MenuItem("Tools/OpenVINO/Refresh")]
    static void Refresh()
    {
        AssetDatabase.Refresh();
        Debug.Log("Refreshing Asset Database.");
    }

    //[MenuItem("Tools/OpenVINO/Attach to Camera")]
    //static void AttachScript()
    //{
    //    GameObject camera = Camera.main.gameObject;

    //    if (camera.GetComponent<StyleTransfer>() == null)
    //    {
    //        camera.AddComponent<StyleTransfer>();
    //    }
    //    else
    //    {
    //        Debug.Log("Script already attached.");
    //    }
    //}

    [MenuItem("Tools/OpenVINO/Move Editor Script")]
    static void MoveEditorScript()
    {
        if (AssetDatabase.IsValidFolder("Assets/Editor") == false)
        {
            AssetDatabase.CreateFolder("Assets", "Editor");
        }
        AssetDatabase.MoveAsset("Assets/OpenVINO/PrepareAssets.cs", "Assets/Editor/PrepareAssets.cs");
    }

}
