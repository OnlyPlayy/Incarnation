using System.Collections;
using UnityEngine;

// Helmet Lion
public class H_Lion : Helmet
{
    [Header("Lion settings")]
    public GameObject attackParticleSystemPrefab;
    public float particlePawForce = 10.0f;
    [Space(5)]
    public GameObject roarSystemPrefab;
    private float roarSpawnDistance = 1.0f;
    public float particleRoarForce = 7.0f;
    private bool leftTurn = false;

    public override void SuperAttack()
    {
        if (canSuperAttack)
        {
            SoundFXManager.instance.PlaySoundFXClip(attackSounds, transform, 1f);

            SpawnPawParticleSystem(transform.position);
            StartCoroutine(RunSuperAttackCooldown(superAttackCooldown));
        }
    }

    public override void SuperAbility()
    {
        if (canSuperAbility)
        {
            SoundFXManager.instance.PlaySoundFXClip(abilityActivateSounds, transform, 1f);

            SpawnRoarSystem(transform.position + transform.forward * roarSpawnDistance);
            StartCoroutine(RunSuperAbilityCooldown(superAbilityCooldown));
        }
    }

    private void SpawnPawParticleSystem(Vector3 spawnPosition)
    {
        float xRotation = transform.rotation.eulerAngles.x;
        float yRotation = transform.rotation.eulerAngles.y - 180.0f;
        float zRotation = 60f; // Paw attack angle 
        if (leftTurn)
            zRotation = 180f - zRotation;

        leftTurn = !leftTurn;
        // Create a new rotation
        Quaternion particleRotation = Quaternion.Euler(new Vector3(xRotation, yRotation, zRotation));

        // Instantiate the particle system prefab
        spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y - 0.5f, spawnPosition.z);
        GameObject particleSystemInstance = Instantiate(attackParticleSystemPrefab, spawnPosition, particleRotation);

        // Make the particle system a child of the object, add rigidbody, apply a forward force to the particle system
        particleSystemInstance.transform.parent = transform.parent.parent;
        Rigidbody particleRigidbody = particleSystemInstance.AddComponent<Rigidbody>();
        particleRigidbody.useGravity = false;
        Vector3 forwardForce = particleSystemInstance.transform.forward * -particlePawForce;
        particleRigidbody.AddForce(forwardForce, ForceMode.Impulse);

        Destroy(particleSystemInstance, 0.5f);
    }

    private void SpawnRoarSystem(Vector3 spawnPosition)
    {
        // Create a new rotation
        Quaternion particleRotation = Quaternion.Euler(transform.rotation.eulerAngles);

        // Instantiate the particle system prefab at the specified position with the calculated rotation
        GameObject systemInstance = Instantiate(roarSystemPrefab, spawnPosition, particleRotation);

        // Add rigidbody, Apply a forward force to the particle system
        Rigidbody particleRigidbody = systemInstance.AddComponent<Rigidbody>();
        particleRigidbody.useGravity = false;
        Vector3 forwardForce = systemInstance.transform.forward * particleRoarForce;
        particleRigidbody.AddForce(forwardForce, ForceMode.Impulse);

        HLA_RoarSpawner roarSystem = systemInstance.GetComponent<HLA_RoarSpawner>();
        float slefDestroyTime = roarSystem.raycastDistance / roarSystem.particleForceMagnitude;

        Destroy(systemInstance, slefDestroyTime);
    }
}