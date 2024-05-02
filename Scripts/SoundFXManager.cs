using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume, float pitch = 1f, float trimSeconds = 0f)
    {
        // don't execute if no array has been assigned
        if (audioClip.Length == 0)
            return;

        // spawn a GameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign random audio clip
        int randomIndex = Random.Range(0, audioClip.Length);
        audioSource.clip = audioClip[randomIndex];

        // set volume and pitch
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        // reset trim if the sound is shorter than trim
        float clipLength = audioSource.clip.length;
        if (clipLength < trimSeconds)
        {
            trimSeconds = 0f;
            Debug.LogWarning("Trim seconds (" + trimSeconds + "s) is bigger than total sound length (" + clipLength + "s), resetting trimSeconds to 0");
        }

        // trim and play
        audioSource.time = trimSeconds;
        audioSource.Play();


        // Destroy
        Destroy(audioSource.gameObject, clipLength - trimSeconds);
    }
}
