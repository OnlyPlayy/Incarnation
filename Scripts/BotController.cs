using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BotController : MonoBehaviour
{
    private PlayerConfiguration playerConfig;
    [SerializeField] private MeshRenderer playerMesh;
    [SerializeField] private Sprite[] playerFaces;

    // prefabs
    [Header("Player Prefabs")]
    [SerializeField] private GameObject visuals;
    [SerializeField] private GameObject capsule;
    [SerializeField] private Transform weapons;
    [SerializeField] private GameObject userInterface;

    [Header("Game Prefabs")]
    [SerializeField] private ParticleSystem bloodParticle;


    // character and movement
    [Header("Character and movement")]
    public float playerSpeed = 5.0f;
    [System.NonSerialized] public float initialPlayerSpeed;
    [System.NonSerialized] public Vector3 playerMovement;

    // ground and ground detection
    [Header("Ground and ground detection")]
    [Tooltip("Player hover over the ground")]
    [SerializeField] private float playerYpos = 1.01f;
    [NonSerialized] public Vector3 playersGroundLevel;
    [Space(3)]
    public int numGroundCheckPoints = 8; 
    public float groundCheckRadius = 0.5f;
    public float groundCheckDistance = 0.1f;
    private float initialPlayerYpos;


    // lockers
    string[] actionLockers = new string[0];

    // attack
    [Header("Attack")]
    [SerializeField] private GameObject stickObject;
    [SerializeField] private float simpleAttackLockTime = 0.15f;
    [SerializeField] private float simpleAttackCooldown = 0.2f;
    [SerializeField] float attackTravelDistance;
    private bool canSimpleAttack = true;
    Animator anim;
    
    [Space(3)]
    [SerializeField] private const float rotateInstantlyDelayTime = 0.05f;

    // positioner variables
    [Header("Positioners")]
    [SerializeField] private GameObject footPositioner;
    [SerializeField] private Color playerColor;

    // sounds
    [Header("Sounds")]
    [SerializeField] private AudioClip[] simpleAttackSounds;
    [SerializeField] private AudioClip[] playerDieSounds;

    // beginning lock
    float roundStartLockDelay = 0.5f;


    // AI implementation
    GameObject player;
    NavMeshAgent agent;
    [SerializeField] LayerMask groundLayer, playerLayer;

    
    // patrol
    Vector3 destPoint;
    bool walkpointSet;
    public bool canPatrol = true;
    [SerializeField] float range = 15, regenerateDestinationDistance = 3;

    //state change
    [SerializeField] float sightRange, attackRange, waitUntil;
    bool playerInSight, playerInAttackRange;
    bool chasePlayer, canDecideToChase;

    private void Start()
    {
        anim = stickObject.GetComponent<Animator>();
        initialPlayerSpeed = playerSpeed;
        agent = GetComponent<NavMeshAgent>();

    }

    // Player Initilizer
    public void InitializePlayer(PlayerConfiguration pc)
    {
        playerConfig = pc;
        playerMesh.material = pc.PlayerMaterial;
        AssignRandomFace();
        playerColor = pc.PlayerColor;
        footPositioner.GetComponent<Renderer>().material.color = playerColor;
        playerConfig.Alive = true;
        StartCoroutine(ActionLockTimer(roundStartLockDelay, "RoundStart"));
    }


    // UPDATE
    private void Update()
    {
        if (FindTheClosestPlayer() != null)
        {
            Vector3 closestPlayerPosition = FindTheClosestPlayer().transform.position;
            playerInSight = Vector3.Distance(transform.position, closestPlayerPosition) < sightRange;
            playerInAttackRange = Vector3.Distance(transform.position, closestPlayerPosition) < attackRange;
        }

        SwitchChasePlayer();

        if (actionLockers.Length == 0)
        {
            agent.enabled = true;
            if (!playerInSight && !chasePlayer) Patrol();
            if (playerInSight || chasePlayer) Chase();
            if (playerInAttackRange) SimpleAttackInitiator();
        } else if (Function.ArrayContainsElement(actionLockers, "tossed"))
        {
            agent.enabled = false;
            transform.position = new Vector3(transform.position.x, playerYpos + 1f, transform.position.z);
        }
        CheckGround();
    }

    void LateUpdate()
    {
        FootDirPositioner();
    }

    void SwitchChasePlayer()
    {
        if (canDecideToChase)
        {
            canDecideToChase = false;
            waitUntil = UnityEngine.Random.Range(1f, 3f) + Time.time;

            chasePlayer = !chasePlayer;
        }
        if (waitUntil < Time.time || waitUntil == 0)
        {
            canDecideToChase = true;
        }
    }


    void Chase()
    {
        if (FindTheClosestPlayer() != null)
        {
            Vector3 playerPosition = FindTheClosestPlayer().transform.position;
            agent.SetDestination(playerPosition);
        }
    }

    // AI logic
    void Patrol()
    {
        if (!walkpointSet) SearchForDest();
        if (walkpointSet) agent.SetDestination(destPoint);
        if (Vector3.Distance(transform.position, destPoint) < regenerateDestinationDistance) walkpointSet = false;
    }

    void SearchForDest()
    {
        float z = UnityEngine.Random.Range(-range, range);
        float x = UnityEngine.Random.Range(-range, range);

        destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

        RaycastHit hit;
        if (Physics.Raycast(destPoint, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            walkpointSet = true;
            Debug.DrawRay(hit.point, Vector3.up * 5f, Color.blue, 1f); // Draw a debug raycast
        }
    }


    // ATTACKS AND ABILITIES
    private void SimpleAttackInitiator()
    {
        // use SIMPLE attack
        if (canSimpleAttack)
        {
            StartCoroutine(SimpleAttack());

            SoundFXManager.instance.PlaySoundFXClip(simpleAttackSounds, transform, 1f);
        }
    }
    private void RotateInstantly()
    {
        GameObject closestPlayer = FindTheClosestPlayer();
        if (closestPlayer != null)
        {
            Vector3 direction = closestPlayer.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
        }
    }

    private GameObject FindTheClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return null;

        float closestDistance = float.MaxValue;
        GameObject closestPlayer = null;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector3.Distance(
                transform.position,
                player.transform.position
            );

            if (distanceToPlayer < closestDistance && player != gameObject)
            {
                closestDistance = distanceToPlayer;
                closestPlayer = player;
            }
        }
        return closestPlayer;
    }

    private IEnumerator SimpleAttack()
    {
        agent.enabled = false;
        canSimpleAttack = false;
        actionLockers = Function.AddElementToArray(actionLockers, "SimpleAttack");

        float waitRandomTime = UnityEngine.Random.Range(0.1f, 0.3f);
        yield return new WaitForSeconds(waitRandomTime);

        RotateInstantly();

        // turn on the stick's collider and set animation's flag
        stickObject.GetComponent<Collider>().enabled = true;
        anim.SetBool("attacking", true);
        yield return new WaitForSeconds(simpleAttackLockTime);//0.267f);

        actionLockers = Function.DeleteElementToArray(actionLockers, "SimpleAttack");
        agent.enabled = true;
        anim.SetBool("attacking", false);
        stickObject.GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(simpleAttackCooldown - simpleAttackLockTime);

        canSimpleAttack = true;
    }



    // CHARACTER SPECIFIC
    private void OnTriggerEnter(Collider collidesWith)
    {
        Debug.Log("Collides with: " + collidesWith.name);
        if (collidesWith.gameObject != gameObject && !collidesWith.transform.IsChildOf(transform))
        {
            if (collidesWith.gameObject.CompareTag("Slow"))
            {
                StartCoroutine(HandleSlow());
            }
            else if (collidesWith.gameObject.CompareTag("DeadlyWeapon"))
            {
                Die(collidesWith.transform);
            } 
        }
    }

    public void Die(Transform collidesWith)
    {
        gameObject.SetActive(false);
        playerConfig.Alive = false;
        PlayerConfigurationManager.Instance.CheckAndUpdatePlayerScore();
        SoundFXManager.instance.PlaySoundFXClip(playerDieSounds, transform, 0.6f);

        // Calculate direction and rotation
        Vector3 direction = collidesWith.position - transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(-direction);
        desiredRotation = Quaternion.Euler(0, desiredRotation.eulerAngles.y, 0);

        // Instantiate blood particle
        ParticleSystem newParticleSystem = Instantiate(bloodParticle, transform.position, desiredRotation);
        Destroy(newParticleSystem, 3f);
    }

    private void FallDeath()
    {
        gameObject.SetActive(false);
        playerConfig.Alive = false;
        PlayerConfigurationManager.Instance.CheckAndUpdatePlayerScore();

        // Instantiate a copy of the capsule at the player's position
        GameObject newCapsule = Instantiate(capsule, transform.position, Quaternion.identity);

        // Apply outward force to the new capsule
        Rigidbody newCapsuleRigidbody = newCapsule.AddComponent<Rigidbody>();
        if (newCapsuleRigidbody != null)
        {
            Vector3 outwardDirection = (newCapsule.transform.position - transform.position).normalized;
            newCapsuleRigidbody.AddForce(outwardDirection * 0.5f, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody component not found on the spawned capsule.");
        }
    }

    private IEnumerator HandleSlow()
    {
        // Slow is used for Lion Super Ability 
        playerSpeed = initialPlayerSpeed * 0.5f;
        agent.speed = playerSpeed;
        yield return new WaitForSeconds(2.0f);
        
        playerSpeed = initialPlayerSpeed;
        agent.speed = playerSpeed;
    }

    public void HandleTossInitiator()
    {
        StartCoroutine(HandleToss());
    }

    private IEnumerator HandleToss()
    {
        actionLockers = Function.AddElementToArray(actionLockers, "tossed");

        float tossDuration = 1.0f;
        float gravity = 9.8f;
        float elapsedTime = 0f;
        float peakYpos = initialPlayerYpos + 2.0f;

        while (elapsedTime < tossDuration)
        {
            // toss
            float t = elapsedTime / tossDuration;
            playerYpos = Mathf.Lerp(initialPlayerYpos, peakYpos, t) - Mathf.Pow(t, 2) * (peakYpos - initialPlayerYpos);

            // gravity
            playerYpos -= gravity * Mathf.Pow(elapsedTime / tossDuration, 2) * Time.deltaTime;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerYpos = initialPlayerYpos;
        actionLockers = Function.DeleteElementToArray(actionLockers, "tossed");
    }

    private void FootDirPositioner()
    {
        footPositioner.SetActive(true);

        // set position 
        Vector3 pos = transform.position;
        footPositioner.transform.position = new Vector3(pos.x, 0.011f, pos.z);
    }

    void CheckGround()
    {
        bool anyGrounded = false;
        bool allGrounded = true;

        // Check each ground check point
        for (int i = 0; i < numGroundCheckPoints; i++)
        {
            // calculate angle and positons
            float angle = (360f / numGroundCheckPoints) * i * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 position = transform.position + direction * groundCheckRadius;

            RaycastHit hit;
            int layerMask = 1 << 10;
            layerMask = ~layerMask;

            if (Physics.Raycast(position, Vector3.down, out hit, groundCheckDistance, layerMask))
            {
                anyGrounded = true;
                Debug.DrawLine(position, hit.point, Color.green);
            }
            else
            {
                allGrounded = false;
                Debug.DrawRay(position, Vector3.down * groundCheckDistance, Color.red);
            }
        }
        if (allGrounded)
        {
            footPositioner.GetComponent<Renderer>().material.color = playerColor;
        }
        else if (anyGrounded)
        {
            footPositioner.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            FallDeath();
        }
    }

    private IEnumerator ActionLockTimer(float lockTime, string name)
    {
        actionLockers = Function.AddElementToArray(actionLockers, name);
        yield return new WaitForSeconds(lockTime);
        actionLockers = Function.DeleteElementToArray(actionLockers, name);
    }
    private void AssignRandomFace()
    {
        if (playerMesh.material != null && playerFaces != null && playerFaces.Length > 0)
        {
            // set random face to the texture
            int randomIndex = UnityEngine.Random.Range(0, playerFaces.Length);
            Texture2D texture = SpriteToTexture(playerFaces[randomIndex]);
            playerMesh.material.mainTexture = texture;
        }
        else
        {
            Debug.LogError("Player material or player faces array is not set or empty.");
        }
    }

    private Texture2D SpriteToTexture(Sprite sprite)
    {
        if (sprite == null)
            return null;

        // Create a new Texture2D with the same dimensions as the sprite
        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

        // Get the pixels from the sprite and apply them to the texture
        texture.SetPixels(sprite.texture.GetPixels(
            (int)sprite.rect.x,
            (int)sprite.rect.y,
            (int)sprite.rect.width,
            (int)sprite.rect.height));

        texture.Apply();

        return texture;
    }
}

