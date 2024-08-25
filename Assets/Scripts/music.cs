using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    public List<AudioClip> tracks = new List<AudioClip>();
    private int trackNum = 0; // Start at the first track
    private AudioSource aSource;

    void Awake()
    {
        // Make sure this object persists between scene loads
        DontDestroyOnLoad(gameObject);

        // Ensure only one instance of this object exists (Singleton pattern)
        if (FindObjectsOfType<Music>().Length > 1)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        aSource = GetComponent<AudioSource>();

        // Shuffle the track list
        ShuffleTracks();

        // Play the first song
        playSong();
    }

    void Update()
    {
        // Check if the current track has finished playing
        if (!aSource.isPlaying)
        {
            // Move to the next track
            trackNum = (trackNum + 1) % tracks.Count; // Loop back to the first track if it's the last one
            playSong();
        }
    }

    void playSong()
    {
        aSource.Stop();
        aSource.clip = tracks[trackNum];
        aSource.Play();
    }

    void ShuffleTracks()
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            AudioClip temp = tracks[i];
            int randomIndex = Random.Range(i, tracks.Count);
            tracks[i] = tracks[randomIndex];
            tracks[randomIndex] = temp;
        }
    }
}
