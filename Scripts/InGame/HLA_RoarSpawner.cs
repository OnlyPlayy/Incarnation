using UnityEngine;

// Helmet Lion Ability Roar Spawner
public class HLA_RoarSpawner : MonoBehaviour
{
    public GameObject roarUnitPrefab;
    public float raycastDistance = 15.0f;
    public float raycastInterval = 0.2f;
    public float particleForceMagnitude = 15f;
    public int numRaycasts = 5;
    public float angleDifference = 15f;

    private float lastRaycastTime;

    void Start()
    {
        lastRaycastTime = Time.time;
    }

    void FixedUpdate()
    {
        if (Time.time - lastRaycastTime >= raycastInterval)
        {
            PerformRaycastArray();
            lastRaycastTime = Time.time;
        }
    }
    
    void PerformRaycastArray()
    {
        float startAngle = -((numRaycasts - 1) * angleDifference) / 2f;  // Calculate the starting angle

        for (int i = 0; i < numRaycasts; i++)
        {
            float currentAngle = startAngle + i * angleDifference;
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 raycastDirection = rotation * Vector3.forward;

            PerformRaycast(raycastDirection);
        }
    }

    void PerformRaycast(Vector3 direction)
    {
        Debug.DrawRay(transform.position, transform.TransformDirection(direction) * raycastDistance, Color.white);
        SpawnRoarUnit(transform.position, direction);
    }

    void SpawnRoarUnit(Vector3 startPosition, Vector3 direction)
    {
        GameObject spawnedObject = Instantiate(roarUnitPrefab, startPosition, Quaternion.identity);

        Vector3 forceDirection = transform.TransformDirection(direction);

        spawnedObject.transform.rotation = Quaternion.LookRotation(forceDirection);

        // Rotate by 90 degrees around Y-axis
        spawnedObject.transform.Rotate(Vector3.up, 90f);

        Rigidbody particleRigidbody = spawnedObject.AddComponent<Rigidbody>();
        particleRigidbody.useGravity = false;
        Vector3 forwardForce = forceDirection * particleForceMagnitude;
        particleRigidbody.AddForce(forwardForce, ForceMode.Impulse);

        float selfDestroyTime = raycastDistance / particleForceMagnitude;
        Destroy(spawnedObject, selfDestroyTime);
    }

}
