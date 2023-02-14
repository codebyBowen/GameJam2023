using UnityEngine;
using System.IO;
using UnityEditor;

public static partial class PlusMusicEditorHelper 
{
    private static GUISkin _uiStyle;
    public static GUISkin uiStyle
    {
        get
        {
            if (_uiStyle != null)
                return _uiStyle;
            _uiStyle = GetUiStyle();
            return _uiStyle;
        }
    }

    private static GUISkin GetUiStyle()
    {
        var searchRootAssetFolder = Application.dataPath;
        var pfGuiPaths = Directory.GetFiles(searchRootAssetFolder, "PlusMusicSkin.guiskin", SearchOption.AllDirectories);
        foreach (var eachPath in pfGuiPaths)
        {
            var loadPath = eachPath.Substring(eachPath.LastIndexOf("Assets"));
            return (GUISkin)AssetDatabase.LoadAssetAtPath(loadPath, typeof(GUISkin));
        }
        return null;
    }
}
