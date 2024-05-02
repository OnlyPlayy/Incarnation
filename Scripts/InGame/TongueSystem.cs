using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Frog- and Chameleon-specific code
public class TongueSystem : MonoBehaviour
{
    // tongue
    public GameObject attackTonguePrefab;
    public float tongueScalingTime = 0.1f;
    public float tongueThickness = 0.2f;

    // detection
    [Space(5)]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float detectionAngle = 50f;

    // sounds
    [SerializeField] private AudioClip[] playerImpactSounds;

    public void LocatePlayers()
    {
        // Locate players within detection distance
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

        List<Transform> nearbyPlayers = new List<Transform>();

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player") && collider.transform != transform)
            {
                nearbyPlayers.Add(collider.transform);
            }
        }

        Transform closestPlayer = GetClosestPlayerWithoutObstacle(nearbyPlayers);
        BotController closestBotScript = null;
        if (closestPlayer != null)
        {
            closestBotScript = closestPlayer.GetComponent<BotController>();
        }
        if (closestPlayer != null)
        {
            // Coroutine to the closest player
            StartCoroutine(TongueScaleAndPosition(transform.position, closestPlayer.position, true, closestBotScript));
        }
        else
        {
            // Coroutine forward until it finds a wall
            Vector3 targetPosition = transform.position + (transform.forward * detectionRadius);
            RaycastHit hit;
            int layerMask = 1 << 7;
            layerMask = ~layerMask;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, detectionRadius, layerMask))
            {
                targetPosition = transform.position + transform.TransformDirection(Vector3.forward) * hit.distance;
            }
            StartCoroutine(TongueScaleAndPosition(transform.position, targetPosition));
        }
    }

    Transform GetClosestPlayerWithoutObstacle(List<Transform> players)
    {
        Transform closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (var player in players)
        {
            if (IsPlayerInDetectionAngle(player))
            {
                // Check obstacles on the way to a player
                Vector3 directionToPlayer = player.position - transform.position;
                RaycastHit hit;

                if (Physics.Raycast(transform.position, directionToPlayer, out hit))
                {
                    if (hit.collider.CompareTag("Player") && hit.collider.transform == player && hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        closestPlayer = player;
                    }
                }
            }
        }
        return closestPlayer;
    }

    bool IsPlayerInDetectionAngle(Transform player)
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Check if the player is within the detection angle
        return angleToPlayer <= detectionAngle * 0.5f;
    }

    private IEnumerator TongueScaleAndPosition(Vector3 startPosition, Vector3 endPosition, bool isPlayer = false, BotController closestBotScript = null)
    {
        // Spawn tongue, and set as child of parent
        GameObject spawnedObject = Instantiate(attackTonguePrefab, startPosition, Quaternion.identity);
        spawnedObject.transform.parent = transform.parent.parent;

        if (isPlayer)
            SoundFXManager.instance.PlaySoundFXClip(playerImpactSounds, transform, 1f);

        // Scale calculations
        float elapsedTime = 0f;
        Vector3 initialScale = new Vector3(tongueThickness, 0f, tongueThickness);
        float distance = Vector3.Distance(startPosition, endPosition);
        float finalScaleFactor = Mathf.Max(distance / 2f, 1f);
        Vector3 finalScale = new Vector3(1f, finalScaleFactor, 1f);
        Vector3 finalScaleXZ = new Vector3(tongueThickness, finalScale.y, tongueThickness);

        // Tongue rotation
        Vector3 directionToPlayer = (endPosition - startPosition).normalized;
        Quaternion rotationToPlayer = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(90f, 0f, 0f);
        spawnedObject.transform.rotation = rotationToPlayer;

        while (elapsedTime < tongueScalingTime)
        {
            // Scale from zero to final scale, move position
            spawnedObject.transform.localScale = Vector3.Lerp(initialScale, finalScaleXZ, elapsedTime / tongueScalingTime);
            Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, 0.5f);
            spawnedObject.transform.position = Vector3.Lerp(startPosition, newPosition, elapsedTime / tongueScalingTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final scale and positon
        spawnedObject.transform.localScale = new Vector3(tongueThickness, finalScaleXZ.y, tongueThickness);
        spawnedObject.transform.rotation = rotationToPlayer;
        spawnedObject.transform.position = Vector3.Lerp(startPosition, endPosition, 0.5f);

        if (closestBotScript != null)
            closestBotScript.Die(spawnedObject.transform);
        yield return null;
        Destroy(spawnedObject, 0.1f);
    }
}