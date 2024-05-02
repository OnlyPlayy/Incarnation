using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopOnObstacle : MonoBehaviour
{
    [SerializeField] private int obstacleLayer = 8;
    [SerializeField] private float castRange = 1.5f;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Get a bit mask
        int layerMask = 1 << obstacleLayer;

        // the Vector3.forward is inverted !!
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(-Vector3.forward), out hit, castRange, layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(-Vector3.forward) * hit.distance, Color.yellow);
            rb.velocity = Vector3.zero;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(-Vector3.forward) * castRange, Color.white);
        }
    }
}
