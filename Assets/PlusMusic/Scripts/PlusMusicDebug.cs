using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlusMusic;
using TMPro;

public class PlusMusicDebug : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text debugText;
    public TMP_Text plusMusicVersion;
    public TMP_Text gameVersion;
    void Start()
    {
        if (PlusMusic_DJ.Instance == null) { Debug.LogError("There is no DJ in scene"); return; }
        PlusMusic_DJ.Instance.RealTimeDebug += ShowDebug;
       
    }

    private void OnDestroy()
    {
        if (PlusMusic_DJ.Instance == null) { return; }
        PlusMusic_DJ.Instance.RealTimeDebug -= ShowDebug;
    }
    public void ShowDebug(string text)
    {
        if (debugText != null)
        {
            debugText.text = text;
        }
        SetVersion();
    }
    public void SetVersion()
    {
        if (PlusMusic_DJ.Instance != null && plusMusicVersion != null)
        {
            plusMusicVersion.text = $"Plugin Version: {PlusMusic_DJ.Instance.PluginVersion}";
        }
        if (gameVersion != null)
        {
            gameVersion.text = $"Game Version: {Application.version}";
        }
    }
}
