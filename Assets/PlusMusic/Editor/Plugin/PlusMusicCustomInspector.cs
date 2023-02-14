using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PlusMusic;

[CustomEditor(typeof(PlusMusic_DJ))]
[System.Serializable]
public class PlusMusicCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PlusMusic_DJ theDJ = (PlusMusic_DJ)target;

        if (GUILayout.Button("Set Soundtrack"))
        {
            theDJ.ChangeSoundtrack(theDJ.WhichSoundtrack);
            Debug.Log("Soundtrack setting to: " + theDJ.WhichSoundtrack);
        }
        if (GUILayout.Button("Stop Sounds"))
        {
            theDJ.theAudioSource.Stop();
            theDJ.newAudio.Stop();
        }
    }
}
