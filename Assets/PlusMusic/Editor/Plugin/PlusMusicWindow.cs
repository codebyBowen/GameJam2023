using PlusMusic;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PlusMusicWindow : EditorWindow
{
    internal static PlusMusicWindow window;

    public static PlusMusicSettingsSo plusMusicSo;


    [MenuItem("PlusMusic/Settings", false, 1)]
    public static void Init()
    {
        var editorAsm = typeof(UnityEditor.Editor).Assembly;
        var inspWndType = editorAsm.GetType("UnityEditor.SceneHierarchyWindow");

        if (inspWndType == null)
        {
            inspWndType = editorAsm.GetType("UnityEditor.InspectorWindow");
        }

        window = GetWindow<PlusMusicWindow>(inspWndType);
        window.titleContent = new GUIContent("PlusMusic");
        window.minSize = new Vector2(320, 0);
    }

    void OnEnable()
    {
        GetSettings();
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        PlusMusicHeader.DrawHeader();
        GUILayout.BeginVertical("", GUI.skin.box);
        PlusMusicSettingsEditor.DrawSettings();
        GUILayout.EndVertical();

        GUILayout.EndVertical();
    }

    private void GetSettings()
    {
        //plusMusicSo = Resources.Load<PlusMusicSettingsSo>("PlusMusic/PlusMusicSettingsSo");
        plusMusicSo = Resources.Load<PlusMusicSettingsSo>("PlusMusicSettingsSo");
        if (plusMusicSo == null)
        {
            plusMusicSo = CreateInstance<PlusMusicSettingsSo>();
            AssetDatabase.CreateAsset(plusMusicSo, "Assets/PlusMusic/Resources/PlusMusicSettingsSo.asset");
            AssetDatabase.SaveAssets();
        }
        ReadJsonPackage();
        PlusMusicSettingsEditor.SetData();
    }
    public static void ReadJsonPackage()
    {
        if (plusMusicSo.jsonPackage == null)
        {
            plusMusicSo.jsonPackage = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/PlusMusic/package.json", typeof(TextAsset));
            if (plusMusicSo.jsonPackage == null)
            {
                Debug.LogError("There is no package.json");
            }
        }
        plusMusicSo.SetPackageData();
    }
}
