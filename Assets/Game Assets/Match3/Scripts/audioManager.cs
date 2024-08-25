using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class audioManager : MonoBehaviour
{
    public static audioManager instance;
    public List<GameObject> sounds = new List<GameObject>();

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void PlaySound(string clipNameOrIndex)
    {
        // Check if the clipName can be parsed to an integer
        if (int.TryParse(clipNameOrIndex, out int index))
        {
            // If it can, use the index to play the sound
            StartCoroutine(PlaySoundByIndex(index));
        }
        else
        {
            // If not, use the clip name to play the sound
            StartCoroutine(PlaySoundByName(clipNameOrIndex));
        }
    }

    IEnumerator PlaySoundByIndex(int index)
    {
        // Instantiate the sound object
        GameObject createNew = Instantiate(sounds[index]);
        createNew.SetActive(false);
        createNew.SetActive(true);

        // Get the AudioSource component from the cloned object
        AudioSource audioSource = createNew.GetComponent<AudioSource>();

        // Wait until the audio has finished playing
        yield return new WaitUntil(() => !audioSource.isPlaying);

        // Destroy the cloned GameObject after the sound has finished playing
        Destroy(createNew);
    }

    IEnumerator PlaySoundByName(string name)
    {
        // Find object by name
        int atIndex = -1;
        int index = 0;
        foreach (GameObject soundObj in sounds)
        {
            if (soundObj.name == name) { atIndex = index; }
            index++;
        }

        // Instantiate the sound object
        GameObject createNew = Instantiate(sounds[index]);
        createNew.SetActive(false);
        createNew.SetActive(true);

        // Get the AudioSource component from the cloned object
        AudioSource audioSource = createNew.GetComponent<AudioSource>();

        // Wait until the audio has finished playing
        yield return new WaitUntil(() => !audioSource.isPlaying);

        // Destroy the cloned GameObject after the sound has finished playing
        Destroy(createNew);
    }
}