using System.Collections;
using UnityEngine;
using System.Collections.Generic;
// Helmet Frog
public class H_Frog : Helmet
{
    [SerializeField] private AudioClip[] groundImpactWithNoElSounds;
    [SerializeField] private AudioClip[] groundImpactWithElementsSounds;
    [SerializeField] private AudioClip[] groundImpactPlayerSounds;

    [Header("Frog settings")]
    [SerializeField] private float jumpHeight = 2.0f;
    [SerializeField] private float jumpTime = 0.5f;
    [SerializeField] GameObject jumpParticle;

    private TongueSystem tongueSystem;

    void Awake()
    {
        tongueSystem = GetComponent<TongueSystem>();
    }

    public override void SuperAttack()
    {
        if (canSuperAttack)
        {
            SoundFXManager.instance.PlaySoundFXClip(attackSounds, transform, 1f);

            tongueSystem.LocatePlayers();
            StartCoroutine(RunSuperAttackCooldown(superAttackCooldown));
        }
    }

    public override void SuperAbility()
    {
        if (canSuperAbility)
        {
            SoundFXManager.instance.PlaySoundFXClip(abilityActivateSounds, transform, 1f);

            StartCoroutine(SuperAbilityJumpAnim());
            StartCoroutine(RunSuperAbilityCooldown(superAbilityCooldown));
        };
    }

    private IEnumerator SuperAbilityJumpAnim()
    {
        float timer = 0f;
        float startY = helmetParent.position.y;

        while (timer < jumpTime)
        {
            float t = timer / jumpTime;
            float firstPhaseT = 0.6f;
            if (t < firstPhaseT)
            {
                float smoothT = 1f - Mathf.Pow(1f - (t / firstPhaseT), 1.5f); // Quadratic ease-out
                float newY = Mathf.Lerp(startY, startY + jumpHeight, smoothT);
                helmetParent.position = new Vector3(helmetParent.position.x, newY, helmetParent.position.z);
            }
            else if (t < 0.9f)
            {
                // hold position in flight
                helmetParent.position = new Vector3(
                    helmetParent.position.x,
                    startY + jumpHeight,
                    helmetParent.position.z);
            }
            else
            {
                // Lerp to ground
                float lerpT = (t - 0.9f) / 0.1f;
                float newY = Mathf.Lerp(startY + jumpHeight, startY, lerpT);
                if (helmetParent != null)
                    helmetParent.position = new Vector3(helmetParent.position.x, newY, helmetParent.position.z);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure a hard finish at the end
        helmetParent.position = new Vector3(helmetParent.position.x, startY, helmetParent.position.z);
        SuperAbilityJumpAffect();
        MakeJumpBurst();
    }

    private void SuperAbilityJumpAffect()
    {
        int segments = 30;
        float radius = 10f; // if changed, remember about the particle>>Shape>>Radius

        float angle = 0f;
        float angleIncrement = 360f / segments;

        Vector3 prevPoint = Vector3.zero;
        Vector3 firstPoint = Vector3.zero;

        // debug circle, draw a circle around the player to show the range of the damage
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            Vector3 currentPoint = transform.position + new Vector3(x, 0f, z);

            // Draw a line between consecutive points
            if (i > 0)
            {
                Debug.DrawLine(prevPoint, currentPoint, Color.green);
            }
            else
            {
                // Save the first point to close the circle
                firstPoint = currentPoint;
            }

            prevPoint = currentPoint;
            angle += angleIncrement;
        }

        // Draw a line to close the circle
        Debug.DrawLine(prevPoint, firstPoint, Color.green);

        // Apply damage to objects with the "player" tag within the range of the circle
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        int playerCount = 0;
        int helmetCount = 0;

        foreach (Collider collider in colliders)
        {
            parentScript = FindPlayerControllerInParents(transform);
            if (collider.gameObject != parentScript.gameObject)
            {
                if (collider.CompareTag("Player"))
                {
                    if (collider.GetComponent<PlayerController>() != null)
                        collider.GetComponent<PlayerController>().HandleTossInitiator();
                    else
                        collider.GetComponent<BotController>().HandleTossInitiator();
                    playerCount++;
                }

                if (collider.CompareTag("Helmet"))
                {
                    Rigidbody rb = collider.GetComponent<Rigidbody>();
                    rb.AddForce(transform.up * 5.3f, ForceMode.Impulse);
                    helmetCount++;
                }
            }
        }

        // play sounds 
        if (playerCount > 0)
            SoundFXManager.instance.PlaySoundFXClip(groundImpactPlayerSounds, transform, 1f);

        if (helmetCount > 1)
            SoundFXManager.instance.PlaySoundFXClip(groundImpactWithElementsSounds, transform, 1f);
        else
            SoundFXManager.instance.PlaySoundFXClip(groundImpactWithNoElSounds, transform, 1f);
    }

    private void MakeJumpBurst()
    {
        Vector3 spawnPosition = parentScript.playersGroundLevel;
        Quaternion qi = Quaternion.identity;
        Quaternion spawnRotation = Quaternion.Euler(new Vector3(qi.x - 90f, qi.y, qi.z));
        GameObject particleSystemInstance = Instantiate(jumpParticle, spawnPosition, spawnRotation);

        Destroy(particleSystemInstance, 1f);
    }
}

