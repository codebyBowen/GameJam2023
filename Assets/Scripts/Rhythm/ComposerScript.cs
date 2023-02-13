using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct SoundObject
{
    public GameObject obj;
    public int noteNumber;
}

public class ComposerScript : MonoBehaviour
{
    public GameObject audioPrefab;

    public ComposeScaleType scaleType = ComposeScaleType.Minor;
    public int baseNote;
    public int numOctaves;
    public bool isChordMode = true;
    public bool isChordProgressionMode = true;
    public bool randomTimeMode = false;
    public float minDelay = 1.5F;
    public float maxDelay = 10;
    public bool fileLoadStats = false;
    
    private float delay;
    private float nextBeatTime;
    private string samplesRootPath = "Samples/";
    private List<SoundObject> sounds;
    private Composer controller;
    private DirectoryInfo dir;
    FileInfo[] info;
    private Note[] nextChordNotes;

    public float rhythmDurationPre = 0.2F;
    public float rhythmDurationPost = 0.5F;
    private float rhythmTime = 0;

    void Start()
    {
        dir = new DirectoryInfo("Assets/Resources/" + samplesRootPath);
        info = dir.GetFiles("*.wav");

        nextBeatTime = Time.fixedTime;
        if (!randomTimeMode) delay = minDelay;

        controller = new Composer(scaleType, baseNote, numOctaves);
        sounds = new List<SoundObject>();

        // scan the directory and load all the sounds in the scale
        ScanDirectory();
    }

    void FixedUpdate()
    {
        Debug.Assert(minDelay > (rhythmDurationPre + rhythmDurationPost));
        // change the chord every couple of seconds
        if (isChordMode)
        {
            if (Time.fixedTime >= nextBeatTime)
            //if(Input.GetKeyDown(KeyCode.L))
            {
                if(!isChordProgressionMode)
                    nextChordNotes = controller.scale.GetRandomChordInScale();
                else
                    nextChordNotes = controller.scale.GetNextChordInChordProgression();

                ManageSamplesBasedOnNextChord();

                if(randomTimeMode) delay = UnityEngine.Random.Range(minDelay, maxDelay);
                nextBeatTime = Time.fixedTime + delay;
                rhythmTime = Time.fixedTime;
            }
        }
    }

    // Must call only in FixedUpdate
    public bool isOnRhythm() {
      return (Time.fixedTime - rhythmTime) <= rhythmDurationPost || (nextBeatTime - Time.fixedTime) <= rhythmDurationPre;
    }

    void ManageSamplesBasedOnNextChord()
    {
        // if the note that's inside the chord isn't loaded before, load the corresponding samples
        // if a note is loaded but it's not in the chord, diactive the corresponding samples
        bool exists;

        foreach (Note n in nextChordNotes)
        {
            exists = false;
            foreach (SoundObject s in sounds)
            {
                if (s.noteNumber == n.number)
                { 
                    exists = true;
                    if (!s.obj.activeSelf)
                    {
                        s.obj.SetActive(true);
                    }
                }
            }

            if (!exists)
            {
                // if note is in the chord but not on the loaded sounds
                LoadFilesWithNote(n.number);
            }
        }

        // if note isn't in the chord but has been loaded before
        int counter = 0;
        foreach (SoundObject s in sounds)
        {
            //if (!nextChord.ChordContainsNote(s.noteNumber))
            if (!Utility.ArrayContains(nextChordNotes, s.noteNumber))
            {
                s.obj.SetActive(false);
            }
            else counter++;
        }
    }

    void ScanDirectory()
    {
        foreach (FileInfo f in info)
        {
            // print("Found: " + f.Name);

            string[] splittedName = f.Name.Split('-');

            int noteNum = Utility.NoteNameToNumber(splittedName[0].ToLower());

            if ((noteNum != -1) && controller.scale.IsInScaleNotes(noteNum))
            {
                GameObject tmp = Instantiate(audioPrefab);
                tmp.GetComponent<AudioSource>().clip = Resources.Load<AudioClip>(samplesRootPath + f.Name.Replace(".wav", ""));
                
                sounds.Add(new SoundObject { noteNumber = noteNum, obj = tmp });

                if (fileLoadStats)
                    print("LOADED: " + f.Name);
            }
        }
    }

    void LoadFilesWithNote(int noteNumber)
    {
        dir = new DirectoryInfo("Assets/Resources/" + samplesRootPath);
        info = dir.GetFiles("*.wav");
        foreach (FileInfo f in info)
        {
            string[] splittedName = f.Name.Split('-');

            int noteNum = Utility.NoteNameToNumber(splittedName[0].ToLower());
            if (noteNum == noteNumber)
            {

                GameObject tmp = Instantiate(audioPrefab);
                tmp.GetComponent<AudioSource>().clip = Resources.Load<AudioClip>(samplesRootPath + f.Name.Replace(".wav", ""));

                sounds.Add(new SoundObject { noteNumber = noteNum, obj = tmp });
                
                if (fileLoadStats)
                    print("LOADED ON RUNTIME: " + f.Name);
            }
        }
    }
}
