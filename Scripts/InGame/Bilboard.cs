using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform parentObject;
    public float forwardOffset = 0f;
    public float upwardOffset = 5f;
    public float sideOffset = 0f;

    void Start()
    {
        parentObject = transform;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            Vector3 directionToCamera = (Camera.main.transform.position - parentObject.position).normalized;
            Vector3 offset = directionToCamera * forwardOffset;
            offset += Vector3.up * upwardOffset;
            offset += Vector3.right * sideOffset;
            transform.localPosition = offset;
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
        else
        {
            Debug.LogWarning("Main camera not found!");
        }
    }
}
