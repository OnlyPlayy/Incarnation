using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] musicClips;
    [NonSerialized] public AudioSource audioSource;
    [NonSerialized] public static MusicManager instance;
    private AudioClip lastPlayedClip;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayRandomClip();
    }

    void Update()
    {
        // Check if the current clip has finished playing
        if (!audioSource.isPlaying)
        {
            PlayRandomClip();
        }
    }

    public void PlayRandomIfOutsideOfScope()
    {
        if (musicClips.Length == 0) return;

        bool ownArrayMusic = false;
        var currentlyPlaying = audioSource.clip;

        for (int i = 0; i < musicClips.Length; i++)
        {
            if (musicClips[i] == currentlyPlaying)
                ownArrayMusic = true;
        }
        if (!ownArrayMusic)
        {
            PlayRandomClip();
        }
    }

    public void PlayRandomClip()
    {
        if (musicClips.Length == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, musicClips.Length);
        audioSource.clip = musicClips[randomIndex];
        audioSource.Play();
    }

    public void PlaySpecifiedClip(AudioClip selectedClip)
    {
        audioSource.clip = selectedClip;
        audioSource.Play();
    }
}
