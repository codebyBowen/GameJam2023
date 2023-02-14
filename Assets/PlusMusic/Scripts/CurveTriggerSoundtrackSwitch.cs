using PlusMusic;
using System;
using UnityEngine;
using static PlusMusic_DJ;

public class CurveTriggerSoundtrackSwitch : MonoBehaviour
{
    [SerializeField] private PlusMusic_DJ theDJ;
    [SerializeField] private TransitionInfo theTransition;
    void Start()
    {
        if (!theDJ) theDJ = Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        //theDJ.PlaySoundPM(switchSoundtrackTag.ToString(), curve, _duration, stingerSoundtrack, useStinger, false, timing);
        theDJ.PlaySoundPM(theTransition);
    }
}
