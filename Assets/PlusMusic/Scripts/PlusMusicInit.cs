using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using PlusMusic;
using System;

public class PlusMusicInit : MonoBehaviour
{
    public Dropdown theDropdownMenu;
    private SoundtrackOptionData[] theSoundtrackOptions;
    // Start is called before the first frame update
    void Start()
    {
        theDropdownMenu.ClearOptions();
        if (PlusMusic_DJ.Instance == null) { Debug.LogError("There is no DJ in scene"); return; }
        PlusMusic_DJ.Instance.OnSoundTrackOptionsReceived += SetSoundtrackOptions;

    }
    private void OnDestroy()
    {
        if (PlusMusic_DJ.Instance == null) { return; }
        PlusMusic_DJ.Instance.OnSoundTrackOptionsReceived -= SetSoundtrackOptions;
    }

    public void SetSoundtrackOptions(SoundtrackOptionData[] soundtrackOptions)
    {
        theSoundtrackOptions = soundtrackOptions;
        theDropdownMenu.ClearOptions();
        List<string> dropOptions = new List<string>();
        foreach (SoundtrackOptionData option in theSoundtrackOptions)
        {
            dropOptions.Add(option.name);
        }
        theDropdownMenu.AddOptions(dropOptions);
    }

    public void SetSoundtrackOnChange()
    {
        string theSoundtrackID = "";
        int forcounter = 0;
        Debug.Log("theDropDownMenu.value = " + theDropdownMenu.value);
        foreach (SoundtrackOptionData option in theSoundtrackOptions)
        {
            if (theDropdownMenu.value == forcounter)
            {
                theSoundtrackID = option.id;
            }
            forcounter++;
        }
        PlusMusic_DJ.Instance.ChangeSoundtrack(theSoundtrackID);
    }

}
