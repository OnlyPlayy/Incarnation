using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundsToPlayWhenCollidesWithPlayer;

    private void OnTriggerEnter(Collider collidesWith)
    {
        if (collidesWith != null)
        {
            if(collidesWith.gameObject.tag == "Player" && !Function.ElementIsChildOf(transform, collidesWith.transform))
            {
                SoundFXManager.instance.PlaySoundFXClip(soundsToPlayWhenCollidesWithPlayer, transform, 1f);
            }
        }
        
    } 
}
