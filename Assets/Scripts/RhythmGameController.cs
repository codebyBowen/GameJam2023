using System.Collections;
using UnityEngine;

public class RhythmGameController : MonoBehaviour
{
    public AudioSource backgroundMusic;

    private float beatInterval;
    private float[] rhythmTimings;
    public GameObject player;
    public GameObject signal;
    public float bpm = 60f;

    void Start()
    {
        // Calculate rhythm timings based on background music clip
        // float bpm = 60f; // Assume a default BPM of 120
        beatInterval = 60f / bpm;
        int numberOfBeats = (int)(backgroundMusic.clip.length /beatInterval);
        rhythmTimings = new float[numberOfBeats];
         
        for (int i = 0; i < numberOfBeats; i++)
        // for (int i = 0; i < rhythmKeys.Length; i++)
        {
            rhythmTimings[i] = beatInterval * i;
        }
        Debug.Log("numberOfBeats" + numberOfBeats);
    }

    void FixedUpdate()
    {
        float musicTime = backgroundMusic.time;
        bool isActive = false;
        bool onRhythm = false;
        for (int i = 0; i < rhythmTimings.Length; i++)
        {
            float timeDiff = musicTime - rhythmTimings[i];
            if (timeDiff >= -beatInterval / 4 && timeDiff <= beatInterval / 4)
            {
                // Debug.Log("Hey!!!");
                isActive = true;
                onRhythm = true;

                if (Input.GetMouseButton(0))
                {
                    // Correct mouse click
                    Debug.Log("Correct mouse click");
                    player.GetComponent<HeroKnight>().onRhythm = true;
                } else {
                    player.GetComponent<HeroKnight>().onRhythm = false;
                }
            }
        }
        player.GetComponent<HeroKnight>().onRhythm = onRhythm;
        signal.SetActive(isActive);
    }
}
