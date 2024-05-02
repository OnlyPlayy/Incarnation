using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private PlayerConfiguration playerConfig;
    [SerializeField] private MeshRenderer playerMesh;
    [SerializeField] private Sprite[] playerFaces;
    private PlayerControlls controlls;
    private bool gameIsPaused;
    private bool showingHelper;

    // prefabs
    [Header("Player Prefabs")]
    [SerializeField] private GameObject visuals;
    [SerializeField] private GameObject capsule;
    [SerializeField] private Transform weapons;
    [SerializeField] private GameObject userInterface;
    public GameObject helper;

    [Header("Game Prefabs")]
    [SerializeField] private ParticleSystem bloodParticle;


    // character and movement
    [Header("Character and movement")]
    [SerializeField] private float roundStartLockDelay = 0.5f;
    [SerializeField] private float turnSmoothTime = 0.03f;
    [SerializeField] private float weaponsTurnSmoothTime = 0.03f;
    public float playerSpeed = 5.0f;
    [System.NonSerialized] public float initialPlayerSpeed;
    [System.NonSerialized] public Vector3 stickDirection;
    [System.NonSerialized] public Vector3 playerMovement;
    private CharacterController controller;
    private Vector3 lastFrameMove;
    private Vector2 movementInput = Vector2.zero;
    private string[] attackLockers = new string[0];
    private float weaponsTurnSmoothVelocity;
    private float turnSmoothVelocity;
    private bool lastFrameCraftButtonPressed = false;

    // ground and ground detection
    [Header("Ground and ground detection")]
    [Tooltip("Player hover over the ground")]
    [SerializeField] private float playerYpos = 1.01f;
    [SerializeField] private float playerYGroundLevel = 0.01f;
    [NonSerialized] public Vector3 playersGroundLevel;
    [Space(3)]
    public int numGroundCheckPoints = 8; 
    public float groundCheckRadius = 0.5f;
    public float groundCheckDistance = 0.1f;
    private float initialPlayerYpos;


    // lockers
    string[] actionLockers = new string[0];
    string[] capsuleRotationLockers = new string[0];
    string[] weaponsRotationLockers = new string[0];


    // dash
    [Header("Dash")]
    [SerializeField] private float dashingTime = 0.4f;
    [SerializeField] private float dashCooldown = 0.4f;
    [SerializeField] private float dashMultiplyer = 4.0f;
    [SerializeField] float dashLerpSpeed = 3f;
    bool canDash = true;
    bool dashing = false;


    // attack
    [Header("Attack")]
    [SerializeField] private GameObject stickObject;
    [SerializeField] private float simpleAttackLockTime = 0.15f;
    [SerializeField] private float simpleAttackCooldown = 0.2f;
    [SerializeField] float attackTravelDistance;
    private bool canSimpleAttack = true;
    private Vector3 lerpAttackTarget;
    Animator anim;
    
    [Space(3)]
    [SerializeField] private float simpleAttackRotationLockTime = 0.267f;
    [SerializeField] private const float rotateInstantlyDelayTime = 0.05f;


    // helmet
    [Header("Helmets")]
    [SerializeField] private GameObject chameleonHelmet;
    [SerializeField] private GameObject frogHelmet;
    [SerializeField] private GameObject lionHelmet;
    [SerializeField] private float reachDistance = 2f;
    private GameObject[] helmetsArray;
    private Helmet wornHelmet = null;


    // whisps
    [Header("Whisps")]
    public int craftingWhispsNeeded = 3;


    // crafting
    [Header("Crafting")]
    [SerializeField] private ParticleSystem craftingParticle;
    [SerializeField] private float smokeDeleteDelay = 2f;
    [SerializeField] private GameObject helmetSelection;
    [SerializeField] private GameObject highlight;
    [SerializeField] private int visualEnhancer = 2;
    [SerializeField] private float craftingTime = 1f;
    private GameObject[] visualsArray;
    private bool smokeSpawned;
    private float helmetSelectionTreshold = 0.8f;


    // positioner variables
    [Header("Positioners")]
    [SerializeField] private GameObject footPositioner;
    [SerializeField] private GameObject positionerPlane;
    private Material positionRingMaterial;
    [SerializeField] private float ringThickness = 0.03f;
    [SerializeField] private Color playerColor;

    // sounds
    [Header("Sounds")]
    [SerializeField] private AudioClip[] simpleAttackSounds;
    [SerializeField] private AudioClip[] helmetSelectionSounds;
    [SerializeField] private AudioClip[] helmetChoiceSounds;
    [SerializeField] private AudioClip[] craftingSounds;
    [SerializeField] private AudioClip[] helmetDropSounds;
    [SerializeField] private AudioClip[] helmetPickUpSounds;
    [SerializeField] private AudioClip[] dashSounds;
    [SerializeField] private AudioClip[] playerDieSounds;

    // sounds flags
    bool helmetSelectionFlag = true;


    // buttons flags
    private bool dashBtn = false;
    private bool pickUpBtn = false;
    private bool attackBtn = false;
    private bool superStateBtn = false;
    private bool abilityBtn = false;
    private float positionTrig = 0.0f;
    private bool superStateActivated = false;
    private bool pauseBtn = false;
    private bool helpBtn = false;   


    private void Awake()
    {
        controlls = new PlayerControlls();
    }

    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        anim = stickObject.GetComponent<Animator>();
        initialPlayerSpeed = playerSpeed;
        initialPlayerYpos = playerYpos;
        positionerPlane.SetActive(true);
        positionRingMaterial = positionerPlane.GetComponent<Renderer>().material;
        helmetsArray = new GameObject[3] { chameleonHelmet, frogHelmet, lionHelmet };
        StartCoroutine(ActionLockTimer(roundStartLockDelay, "RoundStart"));
    }


    // MOVEMENT, AND PLAYER CONTROL
    public void OnMove(CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnDash(CallbackContext context)
    {
        dashBtn = context.action.triggered;
    }

    public void OnPickUp(CallbackContext context)
    {
        pickUpBtn = context.action.triggered;
    }

    public void OnAttack(CallbackContext context)
    {
        attackBtn = context.action.triggered;
    }

    public void OnSuperState(CallbackContext context)
    {
        superStateBtn = context.action.triggered;
    }

    public void OnAbility(CallbackContext context)
    {
        abilityBtn = context.action.triggered;
    }

    public void OnPositioner(CallbackContext context)
    {
        positionTrig = context.ReadValue<float>();
    }

    public void OnPause(CallbackContext context)
    {
        pauseBtn = context.action.triggered;
    }

    public void OnHelp(CallbackContext context)
    {
        helpBtn = context.action.triggered;
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

        SpawnSelectedHelmet(playerConfig.Helmet);
        StartCoroutine(PickUpDelayed());
        StartCoroutine(ActivationDelayed());

        playerConfig.Input.onActionTriggered += Input_onActionTriggered;
    }

    private void Input_onActionTriggered(CallbackContext obj)
    {
        if (obj.action.name == controlls.PlayerGameplay.Movement.name)
        {
            OnMove(obj);
        }
        if (obj.action.name == controlls.PlayerGameplay.Dash.name)
        {
            OnDash(obj);
        }
        if (obj.action.name == controlls.PlayerGameplay.PickUp.name)
        {
            OnPickUp(obj);
        }
        if (obj.action.name == controlls.PlayerGameplay.SimpleAttack.name)
        {
            OnAttack(obj);
        }
        if (obj.action.name == controlls.PlayerGameplay.SuperState.name)
        {
            OnSuperState(obj);
        }
        if (obj.action.name == controlls.PlayerGameplay.SecondWeapon.name)
        {
            OnAbility(obj);
        }
        if (obj.action.name == controlls.PlayerGameplay.Positioner.name)
        {
            OnPositioner(obj);
        }
        if (obj.action.name == controlls.PlayerGameplay.Pause.name)
        {
            OnPause(obj);
        }
        if (obj.action.name == controlls.PlayerGameplay.Help.name)
        {
            OnHelp(obj);
        }
    }


    // UPDATE
    private void Update()
    {
        GatherInput();
        if (!gameIsPaused)
        {
            Movement();

            if (actionLockers.Length == 0)
            {

                // Dash button A / Space
                if (dashBtn && stickDirection.magnitude >= 0.8f && canDash)
                {
                    // Dash
                    StartCoroutine(Dash());
                    dashBtn = false;
                }

                // Pick up button RB / E
                if (pickUpBtn)
                {
                    // Pick up
                    PickUp();
                    pickUpBtn = false;
                }

                // Attack button X / LPM
                if (attackBtn)
                {
                    if (wornHelmet != null)
                    {
                        if (!wornHelmet.superStateActivated)
                        {
                            // Simple attack
                            SimpleAttackInitiator();
                        }

                        else if (wornHelmet.superStateActivated)
                        {
                            // Super attack
                            SuperAttackInitiator();
                        }
                    }
                    else
                    {
                        // Simple attack
                        SimpleAttackInitiator();
                    }
                    attackBtn = false;
                }

                // Super state button B / R
                if (superStateBtn)
                {
                    if (dashing)
                    {
                        dashing = false;
                        superStateBtn = false;
                    }
                    else if (wornHelmet == null)
                    {
                        if (playerConfig.WhispCount < craftingWhispsNeeded)
                        {
                            // Smoke spawn
                            SpawnSmoke(playerConfig.WhispCount + 1, craftingWhispsNeeded);
                            superStateBtn = false;
                        }
                        else if (!smokeSpawned)
                        {
                            // Smoke spawn (got enough whisps)
                            SpawnSmoke(playerConfig.WhispCount + 1, craftingWhispsNeeded);
                            smokeSpawned = true;
                        }
                    }
                    else
                    {
                        // Toggle helmet activation
                        ActivateHelmet();
                        superStateBtn = false;
                    }
                }
                else
                {
                    smokeSpawned = false;
                }

                // Ability button Y / Q
                if (abilityBtn)
                {
                    if (!superStateActivated)
                    {
                        // use simple ability
                    }
                    else
                    {
                        // Super ability
                        SuperAbilityInitiator();
                    }
                }

                // Help button D-pad UP / Z
                if (helpBtn)
                {
                    StartCoroutine(ShowHelpBoard());
                }
            }

            // Show selection menu, button B / R
            if (superStateBtn && Function.ArrayContainsElementsNoOtherThan(actionLockers, "CraftingSelection") && playerConfig.WhispCount >= craftingWhispsNeeded && wornHelmet == null)
            {
                // Show helmet selection
                ShowHelmetSelectionMenu(true);
                actionLockers = Function.AddElementToArray(actionLockers, "CraftingSelection");
                ShowHelmetMenuHighlight();
                lastFrameCraftButtonPressed = true;

                if (helmetSelectionFlag)
                    SoundFXManager.instance.PlaySoundFXClip(helmetSelectionSounds, transform, 1f);
                    helmetSelectionFlag = false;
            }
            else if (!superStateBtn && lastFrameCraftButtonPressed && stickDirection.magnitude > helmetSelectionTreshold)
            {
                // Craft helmet
                lastFrameCraftButtonPressed = false;
                StartCoroutine(CraftHelmet());

                SoundFXManager.instance.PlaySoundFXClip(helmetChoiceSounds, transform, 1f);
                SoundFXManager.instance.PlaySoundFXClip(craftingSounds, transform, 1f);
            }
            else
            {
                // Hide helmet selection
                ShowHelmetSelectionMenu(false);
                actionLockers = Function.DeleteElementToArray(actionLockers, "CraftingSelection");
                lastFrameCraftButtonPressed = false;
                helmetSelectionFlag = true;
            }

            AdjustGroundLevel(playerYGroundLevel);
            RotateWeapons();
            RotateCapsule();
            CheckGround();
        }
        else
        {
            if(PauseMenu.GameIsPaused != gameIsPaused)
            {
                gameIsPaused = PauseMenu.GameIsPaused;
            }
            PauseMenu.NavigationButtons(new Vector2(stickDirection.x, stickDirection.z));
            if (dashBtn || pickUpBtn)
                PauseMenu.ExecuteSelectedButton();
            if (superStateBtn)
                PauseMenu.GameIsPaused = false;
        }


        // Pause, button Start / Escape
        if (pauseBtn)
        {
            PauseHandler();
            pauseBtn = false;
        }

    }

    void LateUpdate()
    {
        RingPositioner();
        FootDirPositioner();
        DebugDrawRay();
        ResetStickPosition();
    }


    private void PauseHandler()
    {
        PauseMenu.GameIsPaused = !PauseMenu.GameIsPaused;
        gameIsPaused = PauseMenu.GameIsPaused;
    }

    private IEnumerator ShowHelpBoard()
    {
        if (!showingHelper)
        {
            showingHelper = true;
            helper.SetActive(true);
            MultipleTargetCamera.UpdateAssignedPlayers();

            yield return new WaitForSeconds(0.2f);

            while (!helpBtn)
            {
                yield return null;
            }

            while (helpBtn)
            {
                yield return null;
            }

            helper.SetActive(false);
            showingHelper = false;
            MultipleTargetCamera.UpdateAssignedPlayers();
        } 
    }


    // ATTACKS AND ABILITIES
    private void SimpleAttackInitiator()
    {
        // use SIMPLE attack
        if (canSimpleAttack)
        {
            dashing = false;
            SetTargetLerp();
            StartCoroutine(SimpleAttack());
            StartCoroutine(WeaponRotationLockTimer(simpleAttackRotationLockTime, "SimpleAttack"));

            SoundFXManager.instance.PlaySoundFXClip(simpleAttackSounds, transform, 1f);
        }
    }

    private Vector3 SetTargetLerp()
    {
        lerpAttackTarget = stickDirection * attackTravelDistance;
        return lerpAttackTarget;
    }

    private void DebugDrawRay()
    {
        Debug.DrawLine(weapons.position, weapons.position + lerpAttackTarget, Color.blue);
    }

    private IEnumerator SimpleAttack()
    {
        canSimpleAttack = false;
        actionLockers = Function.AddElementToArray(actionLockers, "SimpleAttack");

        StartCoroutine(RotateWeaponsInstantly(true));

        // turn on the stick's collider and set animation's flag
        stickObject.GetComponent<Collider>().enabled = true;
        anim.SetBool("attacking", true);
        yield return new WaitForSeconds(simpleAttackLockTime);//0.267f);

        actionLockers = Function.DeleteElementToArray(actionLockers, "SimpleAttack");
        anim.SetBool("attacking", false);
        stickObject.GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(simpleAttackCooldown - simpleAttackLockTime);

        canSimpleAttack = true;
    }

    private void SuperAttackInitiator()
    {
        // use super attack
        if (wornHelmet.canSuperAttack)
        {
            StartCoroutine(RotateWeaponsInstantly());
            wornHelmet.SuperAttack();
            StartCoroutine(ActionLockTimer(wornHelmet.superAttackLockTime, "SuperAttack"));
            StartCoroutine(WeaponRotationLockTimer(wornHelmet.superAttackRotationLockTime, "SuperAttack"));
        }
    }

    private void SuperAbilityInitiator()
    {
        // use super ability
        if (wornHelmet.canSuperAbility)
        {
            StartCoroutine(RotateWeaponsInstantly());
            wornHelmet.SuperAbility();
            StartCoroutine(ActionLockTimer(wornHelmet.superAbilityLockTime, "SuperAbility"));
            StartCoroutine(WeaponRotationLockTimer(wornHelmet.superAbilityRotationLockTime, "SuperAbility"));
        }
    }


    // LOCK TIMERS
    private IEnumerator WeaponRotationLockTimer(float lockTime, string name)
    {
        weaponsRotationLockers = Function.AddElementToArray(weaponsRotationLockers, name);
        yield return new WaitForSeconds(lockTime);
        weaponsRotationLockers = Function.DeleteElementToArray(weaponsRotationLockers, name);
    }

    private IEnumerator ActionLockTimer(float lockTime, string name)
    {
        StartCoroutine(RotateWeaponsInstantly());
        actionLockers = Function.AddElementToArray(actionLockers, name);
        yield return new WaitForSeconds(lockTime);
        actionLockers = Function.DeleteElementToArray(actionLockers, name);
    }


    // MOVEMENT AND ROTATION
    private void GatherInput()
    {
        stickDirection = new Vector3(movementInput.x, 0, movementInput.y);
        playerMovement = stickDirection;
    }

    private void Movement()
    {
        Vector3 playerMovement = stickDirection;

        if (Function.ArrayContainsElement(actionLockers, "SimpleAttack"))
        {
            // Simple Attack
            // Lerping towards attack target
            var attackStartPoint = lastFrameMove * 2 + playerMovement;
            playerMovement = Vector3.Lerp(attackStartPoint, lerpAttackTarget, Time.deltaTime).normalized;

            // Move slightly
            controller.Move(playerMovement * Time.deltaTime * initialPlayerSpeed /2);
        }
        else if (dashing && actionLockers.Length == 0)
        {
            // Lerping towards stickDirection
            playerMovement = Vector3.Lerp(lastFrameMove, stickDirection, dashLerpSpeed * Time.deltaTime).normalized;

            // Dash
            controller.Move(playerMovement * dashMultiplyer * Time.deltaTime * initialPlayerSpeed);
        }
        else if (actionLockers.Length == 0)
        {
            // Regular movement
            controller.Move(playerMovement * Time.deltaTime * playerSpeed);
        }

        // Update lastFrameMove regardless of dashing state
        lastFrameMove = playerMovement;
    }



    private IEnumerator Dash()
    {
        SoundFXManager.instance.PlaySoundFXClip(dashSounds, transform, 1f);

        // dash logic [that logic is smart, bc it plays by its own rules]
        dashing = true;
        canDash = false;
        yield return new WaitForSeconds(dashingTime);
        dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void RotateCapsule()
    {
        // rotate capsule
        if (playerMovement.magnitude >= 0.4f && capsuleRotationLockers.Length == 0)
        {
            // smooth rotate to movement direction
            float targetAngle = Mathf.Atan2(playerMovement.x, playerMovement.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                capsule.gameObject.transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                !dashing ? turnSmoothTime : turnSmoothTime / dashMultiplyer
            );
            capsule.gameObject.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    private void RotateWeapons()
    {
        // rotate weapons
        if (playerMovement.magnitude >= 0.05f && weaponsRotationLockers.Length == 0)
        {
            float weaponsTargetAngle = Mathf.Atan2(playerMovement.x, playerMovement.z) * Mathf.Rad2Deg;
            float weaponsAngle = Mathf.SmoothDampAngle(
                weapons.gameObject.transform.eulerAngles.y,
                weaponsTargetAngle,
                ref weaponsTurnSmoothVelocity,
                !dashing ? weaponsTurnSmoothTime : weaponsTurnSmoothTime / dashMultiplyer
            );
            weapons.rotation = Quaternion.Euler(0f, weaponsAngle, 0f).normalized;
        }
    }

    private IEnumerator RotateWeaponsInstantly(bool setTargetLerp = false, float rotationDelay = rotateInstantlyDelayTime)
    {
        yield return null;
        yield return new WaitForSeconds(rotationDelay);

        Vector3 newRotation = new Vector3(stickDirection.x, 0, stickDirection.y);

        // rotate weapons
        if (newRotation.magnitude >= 0.05f)
        {
            float targetRotation = Mathf.Atan2(stickDirection.x, stickDirection.z) * Mathf.Rad2Deg;
            capsule.transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);
            weapons.rotation = Quaternion.Euler(0f, targetRotation, 0f).normalized;
        }

        if (setTargetLerp) SetTargetLerp();
    }

    private void ResetStickPosition()
    {
        if(!anim.GetBool("attacking"))
            stickObject.transform.localPosition = new Vector3(0, 0, 0.8f);
    }


    // CHARACTER SPECIFIC
    private void OnTriggerEnter(Collider collidesWith)
    {
        if (collidesWith.gameObject != gameObject && !collidesWith.transform.IsChildOf(transform))
        {
            if (collidesWith.gameObject.CompareTag("Slow"))
            {
                StartCoroutine(HandleSlow());
            }
            else if (collidesWith.gameObject.CompareTag("Whisp"))
            {
                Debug.Log("A whisp");
            }
            else if (collidesWith.gameObject.CompareTag("DeadlyWeapon"))
            {
                HandleDamage(collidesWith);
            } 
            else
            {
                dashing = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        dashing = false;
    }

    public void HandleDamage(Collider collidesWith)
    {
        // Drop helmet or kill on attack
        if (wornHelmet != null)
        {
            PickUp();
        }
        else
        {
            Die(collidesWith);
        }
    }

    private void Die(Collider collidesWith)
    {
        gameObject.SetActive(false);
        playerConfig.Alive = false;
        playerConfig.Helmet = null;
        PlayerConfigurationManager.Instance.CheckAndUpdatePlayerScore();
        SoundFXManager.instance.PlaySoundFXClip(playerDieSounds, transform, 0.6f);

        // Calculate direction and rotation
        Vector3 direction = collidesWith.transform.position - transform.position;
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
        playerConfig.Helmet = null;
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
        dashing = false;

        // Slow is used for Lion Super Ability 
        playerSpeed = initialPlayerSpeed * 0.5f;
        yield return new WaitForSeconds(2.0f);
        if (superStateActivated)
        {
            playerSpeed = initialPlayerSpeed * wornHelmet.activationPlayerSpeedMultiplyer;
        }
        else
        {
            playerSpeed = initialPlayerSpeed;
        }

    }

    public void HandleTossInitiator()
    {
        StartCoroutine(HandleToss());
    }

    private IEnumerator HandleToss()
    {
        dashing = false;
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

    public void SetArrayActiveState(GameObject[] objects, bool isActive)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(isActive);
        }
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

    private void AdjustGroundLevel(float newGroundLevel)
    {

        // reset y value
        if (controller.transform.position.y != playerYpos)
        {
            controller.enabled = false;
            Vector3 pos = controller.transform.position;
            pos.y = playerYpos;
            controller.transform.position = pos;
            controller.enabled = true;
        }
        // adjust ground level for the effects
        playersGroundLevel = new Vector3(transform.position.x, newGroundLevel, transform.position.z);
    }

    private void RingPositioner()
    {
        float innerRadius = (positionTrig / 2) - ringThickness;
        float outerRadius = positionTrig / 2;

        positionRingMaterial.SetFloat("_InnerRadius", innerRadius);
        positionRingMaterial.SetFloat("_OuterRadius", outerRadius);
        positionRingMaterial.SetFloat("_Thickness", 0.0f);
        positionRingMaterial.SetColor("_Color", playerColor);

        positionerPlane.transform.LookAt(Camera.main.transform);
        positionerPlane.transform.rotation *= Quaternion.Euler(90, 0, 0);
    }

    private void FootDirPositioner()
    {
        if (wornHelmet == null || !wornHelmet.hidden)
        {
            footPositioner.SetActive(true);

            // set position 
            Vector3 pos = transform.position;
            footPositioner.transform.position = new Vector3(pos.x, 0.011f, pos.z);

            // rotate while moving
            if (stickDirection != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(stickDirection) * Quaternion.Euler(0f, -135f, 0f);
                footPositioner.transform.rotation = rotation;
            }
        }
        else
        {
            footPositioner.SetActive(false);
        }
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
        if (!dashing)
        {
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
    }


    // WHISP SPECIFIC
    //private void HandleWhispCollection(GameObject whisp)
    //{
    //    if (whisp.GetComponent<Whisp>().canPatrol)
    //    {
    //        WhispCollected();
    //    }
    //}

    public void WhispCollected()
    {
        if (playerConfig.WhispCount < craftingWhispsNeeded)
        {
            playerConfig.WhispCount++;
            Debug.Log("playerConfig.WhispCount " + playerConfig.WhispCount);
        }
    }


    // HELMET SPECIFIC
    public void PickUp()
    {
        // get every helmet GameObject
        GameObject[] helmets = GameObject.FindGameObjectsWithTag("Helmet");

        if (helmets.Length == 0)
        {
            Debug.Log("No helmets found");
            return;
        }

        float closestDistance = float.MaxValue;
        GameObject closestHelmet = null;

        // iterate through each helmet and find the closest one
        foreach (GameObject helmet in helmets)
        {
            float distanceToHelmet = Vector3.Distance(
                transform.position,
                helmet.transform.position
            );

            if (distanceToHelmet < closestDistance && distanceToHelmet <= reachDistance)
            {
                closestDistance = distanceToHelmet;
                closestHelmet = helmet;
            }
        }

        if (closestHelmet != null)
        {
            Debug.Log("Closest helmet found!");

            // Access the Helmet component and pick it up
            Helmet helmetScript = closestHelmet.GetComponent<Helmet>();
            if (helmetScript.isPickedUp)
            {
                // Drop helmet
                if (superStateActivated)
                {
                    // Deactivate super state if needed
                    superStateActivated = wornHelmet.DeactivateSuperState(false);
                    visualsArray = CountNonHelmetVisibles();
                    SetArrayActiveState(visualsArray, true);
                }

                helmetScript.Drop();
                wornHelmet = null;

                SoundFXManager.instance.PlaySoundFXClip(helmetDropSounds, transform, 1f);
            }
            else if (helmetScript != null)
            {
                // Pick up helmet
                helmetScript.PickUp(weapons.transform);
                wornHelmet = helmetScript;

                SoundFXManager.instance.PlaySoundFXClip(helmetPickUpSounds, transform, 1f);
            }
        }
        else
        {
            Debug.Log("No helmet in reach distance");
        }
    }

    private void ActivateHelmet() {
        if (wornHelmet == null) return;
        if (!wornHelmet.superStateActivated)
        {
            // activate super state
            superStateActivated = wornHelmet.ActivateSuperState();

            visualsArray = CountNonHelmetVisibles();
            SetArrayActiveState(visualsArray, false);

        }
        else
        {
            // deactivate super state
            superStateActivated = wornHelmet.DeactivateSuperState();

            visualsArray = CountNonHelmetVisibles();
            SetArrayActiveState(visualsArray, true);
        }
    }

    private GameObject[] CountNonHelmetVisibles()
    {
        Transform parentTransform = weapons.transform;
        List<GameObject> childrenList = new List<GameObject>();

        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform childTransform = parentTransform.GetChild(i);
            if (!childTransform.gameObject.CompareTag("Helmet"))
            {
                childrenList.Add(childTransform.gameObject);
            }
            childrenList.Add(capsule);
        }

        return childrenList.ToArray();
    }


    // CRAFTING SPECIFIC
    private void ShowHelmetSelectionMenu(bool setActive)
    {
        helmetSelection.SetActive(setActive);
    }

    private void ShowHelmetMenuHighlight()
    {
        RectTransform highlightRectTransform = highlight.GetComponent<RectTransform>();
        float highlightOffset = 150f;
        float highlightSpeed = 20f;

        // Update highlight position
        float targetX = stickDirection.x * highlightOffset;
        float targetY = stickDirection.z * highlightOffset;

        // Check if stick direction magnitude is higher than treshold for snapping
        if (stickDirection.magnitude > helmetSelectionTreshold)
        {
            // Calculate angle based on stick direction
            float angle = Mathf.Atan2(stickDirection.x, stickDirection.z) * Mathf.Rad2Deg + 180;
            Debug.Log(angle);
            float adjustment = -30f; //hotfix (btw, it works, leave it, just those angles are wierd)
            
            // Snap angle
            if (angle > 120f && angle <= 240f)
            {
                angle = 300f + adjustment;
                Debug.Log("Chameleon angle");
            }
            else if (angle > 240f && angle <= 360f)
            {
                angle = 180f + adjustment;
                Debug.Log("Frog angle");
            }
            else if (angle >= 0f && angle <= 120f)
            {
                angle = 60f + adjustment;
                Debug.Log("Lion angle");
            }

            // Convert angle back to radians
            float radians = (angle - 180) * Mathf.Deg2Rad;
            targetX = Mathf.Cos(radians) * highlightOffset;
            targetY = Mathf.Sin(radians) * highlightOffset;
        }

        // Lerp to new position
        Vector2 newPosition = Vector2.Lerp(highlightRectTransform.anchoredPosition, new Vector2(targetX, targetY), Time.deltaTime * highlightSpeed);
        highlightRectTransform.anchoredPosition = newPosition;

        // Scale up on magnitude higher than treshold
        float scaleFactor = stickDirection.magnitude > helmetSelectionTreshold ? 2f : 1f;
        Vector3 newScale = Vector3.Lerp(highlightRectTransform.localScale, Vector3.one * scaleFactor, Time.deltaTime * highlightSpeed);
        highlightRectTransform.localScale = newScale;
    }

    private IEnumerator CraftHelmet()
    {
        // Prepare helmet, get the angle
        float angle = Mathf.Atan2(stickDirection.x, stickDirection.z) * Mathf.Rad2Deg + 180;
        GameObject helmetObject = null;

        if (stickDirection.magnitude > helmetSelectionTreshold)
        {
            // chameleon selection
            if (120f < angle && angle <= 240f)
            {
                helmetObject = helmetsArray[0];
            }
            // frog selection
            else if (240f < angle && angle <= 360f)
            {
                helmetObject = helmetsArray[1];
            }
            // lion selection
            else if (0f <= angle && angle <= 120f)
            {
                helmetObject = helmetsArray[2];
            }
        }
        yield return null;

        int sections = 3;

        SpawnSmoke(playerConfig.WhispCount + visualEnhancer, craftingWhispsNeeded);
        actionLockers = Function.AddElementToArray(actionLockers, "CraftingHelmet");
        yield return new WaitForSeconds(craftingTime / sections);

        SpawnSmoke(playerConfig.WhispCount + visualEnhancer, craftingWhispsNeeded);
        yield return new WaitForSeconds(craftingTime / sections);

        SpawnSmoke(playerConfig.WhispCount + visualEnhancer, craftingWhispsNeeded);
        yield return new WaitForSeconds(craftingTime / sections);

        SpawnSelectedHelmet(helmetObject);

        playerConfig.WhispCount -= craftingWhispsNeeded;
        actionLockers = Function.DeleteElementToArray(actionLockers, "CraftingHelmet");

        // pick up crafted helmet
        yield return null;
        StartCoroutine(PickUpDelayed());
        StartCoroutine(ActivationDelayed());
    }

    private IEnumerator PickUpDelayed()
    {
        yield return null;
        PickUp();
    }
    private IEnumerator ActivationDelayed()
    {
        yield return null;
        ActivateHelmet();
    }

    private GameObject SpawnSelectedHelmet(GameObject selectedHelmet)
    {
        Debug.Log("Spawning helmet: " + selectedHelmet);
        playerConfig.Helmet = selectedHelmet;
        if (selectedHelmet == null) return null;
        return Instantiate(selectedHelmet, transform.position, Quaternion.identity);
    }

    private void SpawnSmoke(int unitsCount, int unitsNeeded)
    {
        Quaternion smokeRotation = Quaternion.identity * Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem localSmoke = Instantiate(craftingParticle, playersGroundLevel, smokeRotation);

        // set size
        var main = localSmoke.main;
        float desiredSize = (float)unitsCount / (float)unitsNeeded;
        main.startSize = desiredSize;

        Destroy(localSmoke.gameObject, smokeDeleteDelay);
    }
}

