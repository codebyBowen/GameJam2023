using System;
using PlusMusic;
using UnityEditor;
using UnityEngine;

public class PlusMusicSettingsEditor : Editor
{
    private static Int64  projectId          = 0;
    private static string apiKey             = "";
    private static float  musicVolume        = 1.0f;
    private static bool   autoLoadProject    = true;
    private static bool   autoPlayProject    = true;
    private static bool   debugMode          = false;
    private static bool   logServerResponses = false;

    public static void SetData()
    {
        if (PlusMusicWindow.plusMusicSo != null)
        {
            projectId          = PlusMusicWindow.plusMusicSo.ProjectId;
            apiKey             = PlusMusicWindow.plusMusicSo.ApiKey;
            musicVolume        = PlusMusicWindow.plusMusicSo.MusicVolume;
            autoLoadProject    = PlusMusicWindow.plusMusicSo.AutoLoadProject;
            autoPlayProject    = PlusMusicWindow.plusMusicSo.AutoPlayProject;
            debugMode          = PlusMusicWindow.plusMusicSo.DebugMode;
            logServerResponses = PlusMusicWindow.plusMusicSo.LogServerResponses;

            // Clamp values
            if (musicVolume < 0.0f) { musicVolume = 0.0f; }
            if (musicVolume > 1.0f) { musicVolume = 1.0f; }
            if (projectId < 0) { projectId = 0; }
        }
    }

    public static void DrawSettings()
    {

        float leftWidth    = 130.0f;
        float spacerHeight = 10.0f;
        float labelHeight  = 25.0f;
        float buttonWidth  = 100.0f;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Welcome to PlusMusic", GUILayout.Height(labelHeight));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Label("Please configure your PlusMusic plugin:", GUILayout.Height(labelHeight));

        GUILayout.BeginHorizontal();
        GUILayout.Label("Default Project Id", GUILayout.Width(leftWidth));
        projectId = EditorGUILayout.LongField(projectId);
        GUILayout.EndHorizontal();
        GUILayout.Space(spacerHeight);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Project Api Key", GUILayout.Width(leftWidth));
        apiKey = EditorGUILayout.TextField(apiKey);
        GUILayout.EndHorizontal();
        GUILayout.Space(spacerHeight);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Music Volume", GUILayout.Width(leftWidth));
        musicVolume = EditorGUILayout.FloatField(musicVolume);
        GUILayout.EndHorizontal();
        GUILayout.Space(spacerHeight);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Auto Load Project", GUILayout.Width(leftWidth));
        autoLoadProject = EditorGUILayout.Toggle(autoLoadProject);
        GUILayout.EndHorizontal();
        GUILayout.Space(spacerHeight);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Auto Play Project", GUILayout.Width(leftWidth));
        autoPlayProject = EditorGUILayout.Toggle(autoPlayProject);
        GUILayout.EndHorizontal();
        GUILayout.Space(spacerHeight);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Debug Mode", GUILayout.Width(leftWidth));
        debugMode = EditorGUILayout.Toggle(debugMode);
        GUILayout.EndHorizontal();
        GUILayout.Space(spacerHeight);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Log Server Responses", GUILayout.Width(leftWidth));
        logServerResponses = EditorGUILayout.Toggle(logServerResponses);
        GUILayout.EndHorizontal();
        GUILayout.Space(spacerHeight);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save", GUILayout.Height(labelHeight), GUILayout.Width(buttonWidth)))
        {
            SaveInformation();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private static void SaveInformation()
    {
        if (PlusMusicWindow.plusMusicSo != null)
        {
            // Clamp values
            if (musicVolume < 0.0f) { musicVolume = 0.0f; }
            if (musicVolume > 1.0f) { musicVolume = 1.0f; }
            if (projectId < 0) { projectId = 0; }

            PlusMusicWindow.plusMusicSo.SaveData(
                projectId, apiKey,
                musicVolume, autoLoadProject, autoPlayProject, debugMode, logServerResponses);

            PlusMusicWindow.ReadJsonPackage();
            SaveSo(PlusMusicWindow.plusMusicSo);
        }
    }

    public static void SaveSo(PlusMusicSettingsSo _instance)
    {
        EditorUtility.SetDirty(_instance);
        AssetDatabase.SaveAssets();
    }
}
