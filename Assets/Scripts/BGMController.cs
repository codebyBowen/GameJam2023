using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using PlusMusic;
using UnityEngine.Audio;

public class BGMController : MonoBehaviour
{
    public bool isPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        isPlaying = false;
        PlusMusic_DJ.Instance.OnSoundTrackOptionsReceived += (SoundtrackOptionData[] options) => {
            // plusMusicDJ.LoadSoundtrack(1687);
            PlusMusic_DJ.Instance.ChangeSoundtrack("1687");
            // plusMusicDJ.PlayArrangement(
            //     new TransitionInfo(plusMusicDJ.autoPlayArrangement, 1.0f, PlusMusic_DJ.PMTimings.bars, "", true)
            // );
            // PlayFunkMeUp();
        };
        // if (plusMusicDJ.TheSoundtrackOptions != null) {
        //     Debug.Log("Loading BGM - Funk Me Up");
        //     plusMusicDJ.LoadSoundtrack(1687);
        //     plusMusicDJ.PlayArrangement(
        //         new TransitionInfo(plusMusicDJ.autoPlayArrangement, 1.0f, PlusMusic_DJ.PMTimings.bars, "", true)
        //     );
        // }
    }

    // Update is called once per frame
    void Update()
    {
        if (PlusMusic_DJ.Instance.TheSoundtrackOptions != null && PlusMusic_DJ.Instance.WhichSoundtrack != "1687" && !isPlaying) {
            PlayFunkMeUp();
            isPlaying = true;
        }
    }

    void PlayFunkMeUp() {
        Debug.Log("Loading BGM - Funk Me Up");
        PlusMusic_DJ.Instance.ChangeSoundtrack("1687");
        // PlusMusic_DJ.Instance.LoadSoundtrack(1687);
        PlusMusic_DJ.Instance.PlayArrangement(
            new TransitionInfo(PlusMusic_DJ.PMTags.victory, 1.0f, PlusMusic_DJ.PMTimings.bars, "", true)
        );
    }
}
