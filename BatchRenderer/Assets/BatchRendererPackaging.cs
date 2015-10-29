#if UNITY_EDITOR
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;


public class FrameCapturerPackaging
{
    [MenuItem("Assets/Ist/BatchRenderer/MakePackage")]
    public static void MakePackage()
    {
        string[] files = new string[]
        {
"Assets/Ist",
"Assets/BatchRendererExamples",
        };
        AssetDatabase.ExportPackage(files, "BatchRenderer.unitypackage", ExportPackageOptions.Recurse);
    }

}
#endif // UNITY_EDITOR
