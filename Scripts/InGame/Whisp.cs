using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Whisp : MonoBehaviour
{
    GameObject player;
    NavMeshAgent agent;

    [SerializeField] GameObject Trail_01, Trail_02;

    [SerializeField] LayerMask groundLayer;

    // patrol
    Vector3 destPoint;
    bool walkpointSet;
    public bool canPatrol = true;
    [SerializeField] float range = 10, regenerateDestinationDistance = 1;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canPatrol) Patrol();
        else FollowPlayer();
    }

    void Patrol()
    {
        if (!walkpointSet) SearchForDest();
        if (walkpointSet) agent.SetDestination(destPoint);
        if (Vector3.Distance(transform.position, destPoint) < regenerateDestinationDistance) walkpointSet = false;
    }

    void FollowPlayer()
    {
        var playerScript = player.GetComponent<PlayerController>();
        if (playerScript == null) return;
        float speed = playerScript.playerSpeed * 1.5f; 
        transform.position = Vector3.Lerp(transform.position, player.transform.position, speed * Time.deltaTime);
    }


    void SearchForDest()
    {
        float z = Random.Range(-range, range);
        float x = Random.Range(-range, range);

        destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

        RaycastHit hit;
        if (Physics.Raycast(destPoint, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            walkpointSet = true;
            Debug.DrawRay(hit.point, Vector3.up * 5f, Color.blue, 1f); // Draw a debug raycast
        }
    }

    private void OnTriggerEnter(Collider collidesWith)
    {
        if (collidesWith.gameObject.CompareTag("Player"))
        {
            SnapToPlayer(collidesWith.gameObject);
        }
    }

    private void SnapToPlayer(GameObject playerObject)
    {
        var playerController = playerObject.GetComponent<PlayerController>();
        if (playerController != null && canPatrol)
        {
            playerController.WhispCollected();
            canPatrol = false;
        }
        player = playerObject;
        StartCoroutine(StopCastingFlames());
        Destroy(gameObject, 4f);
    }

    private IEnumerator StopCastingFlames()
    {
        yield return null;
        canPatrol = false;
        var main1 = Trail_01.GetComponent<ParticleSystem>().main;
        var main2 = Trail_02.GetComponent<ParticleSystem>().main;

        main1.startLifetime = 0f;
        main2.startLifetime = 0f;
    }
}
