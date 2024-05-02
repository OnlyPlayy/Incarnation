using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MultipleTargetCamera : MonoBehaviour
{
    public List<Transform> targets = new List<Transform>();

    public Vector3 offset = new Vector3(0, 18.9f, -40);
    public float smoothTime = 0.3f;

    public float minZoom = 45f;
    public float maxZoom = 18f;
    public float zoomLimiter = 45f;

    private Vector3 velocity;
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (targets.Count == 0)
            return;

        Move();
        Zoom();
    }

    void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 1; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.size.x;
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }

    public static void UpdateAssignedPlayers()
    {
        // Find the Main Camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Check if the script is attached
            MultipleTargetCamera cameraScript = mainCamera.GetComponent<MultipleTargetCamera>();
            if (cameraScript == null)
            {
                cameraScript = mainCamera.gameObject.AddComponent<MultipleTargetCamera>();
            }

            // Find players
            cameraScript.targets.Clear();
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            var playerTransforms = new List<Transform>();
            foreach (GameObject player in players)
            {
                if (player.name == "Player(Clone)" || player.name == "Cup(Clone)" || player.name == "Helper" || player.name == "PlayerBot(Clone)")
                {
                    playerTransforms.Add(player.transform);
                }
            }

            // Add players to the camera
            foreach (Transform playerTransform in playerTransforms)
            {
                if (playerTransform.gameObject.activeSelf && playerTransform != null)
                    cameraScript.targets.Add(playerTransform);
            }
        }
        else
        {
            Debug.LogWarning("Main Camera not found in the scene.");
        }
    }
}
