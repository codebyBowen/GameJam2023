using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlusMusicHeader : UnityEditor.Editor
{
    public static new void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("", PlusMusicEditorHelper.uiStyle.GetStyle("pmLogo"), GUILayout.MaxHeight(80), GUILayout.Width(80));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
}
