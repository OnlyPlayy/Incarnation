using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRotate : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] bool rotateX = false;
    [SerializeField] bool rotateY = false;
    [SerializeField] bool rotateZ = false;

    // Update is called once per frame
    void Update()
    {
        // Rotate the object around the X-axis
        if (rotateX)
            transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

        // Rotate the object around the Y-axis
        if (rotateY)
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Rotate the object around the Z-axis
        if (rotateZ)
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
