using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class cup : MonoBehaviour
{
    [SerializeField] float dropDistance = 10f;
    [SerializeField] AudioClip[] sound;

    void Start()
    {
        transform.position = transform.position + new Vector3(0, dropDistance, 0); 
        MultipleTargetCamera.UpdateAssignedPlayers();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("DeadlyWeapon") || other.gameObject.CompareTag("Player"))
        {
            SoundFXManager.instance.PlaySoundFXClip(sound, transform, 1f);
            PlayerConfigurationManager.Instance.AwakeNextMapLoad("MainMenu");
        }
    }
}
