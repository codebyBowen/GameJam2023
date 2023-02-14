using UnityEditor;
using UnityEngine;
namespace PlusMusic
{
    [InitializeOnLoad]
    class WebGLBuildAdjustments
    {
        static WebGLBuildAdjustments()
        {
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.threadsSupport = false;
            PlayerSettings.WebGL.memorySize = 256;
        }
    }
}