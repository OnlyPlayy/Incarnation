using UnityEngine;
using System.Collections;
using static UnityEngine.InputSystem.InputAction;

public class Helmet : MonoBehaviour
{
    // flags
    [System.NonSerialized] public bool isPickedUp = false;
    [System.NonSerialized] public bool superStateActivated = false;
    [System.NonSerialized] public bool canSuperAttack = true;
    [System.NonSerialized] public bool canSuperAbility = true;
    [System.NonSerialized] public Transform helmetParent;
    [System.NonSerialized] public PlayerController parentScript;
    private Transform originalParent;
    private Rigidbody rb;

    // helmet
    [Header("Helmet")]
    public Vector3 helmetPos = new Vector3(0, 0.7f, 0);
    [SerializeField] private float dropForwardForce = 2.5f;
    [SerializeField] private float dropUpwardForce = 3.0f;
    [SerializeField] private float dropTorque = 10.0f;
    [Space(5)]
    [Tooltip("Multiply helmet size and scale Speed by x")]
    public float activationScaleSpeed = 5f;
    public float initialScaleMultiplyer = 1.1f;
    [Tooltip("Multiply helmet size by x when helmet activated")]
    public float activationScaleMultiplyer = 1.8f;
    [Tooltip("Multiply player's speed by x when helmet activated")]
    public float activationPlayerSpeedMultiplyer = 1.2f;

    [Header("Locks and cooldowns")]
    [Space(5)]
    public float superAttackCooldown = 0.4f;
    public float superAttackLockTime = 0.4f;
    public float superAttackRotationLockTime = 0.4f;
    [Space(3)]
    public float superAbilityCooldown = 1.0f;
    public float superAbilityLockTime = 1.0f;
    public float superAbilityRotationLockTime = 1.0f;


    // sounds
    [Header("Sounds")]
    [SerializeField] private AudioClip[] helmetActivateSounds;
    [SerializeField] private AudioClip[] helmetDeactivateSounds;
    public AudioClip[] abilityActivateSounds;
    public AudioClip[] abilityDeactivateSounds;
    public AudioClip[] attackSounds;

    // for chameleon only (atm)
    [System.NonSerialized] public bool hidden = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;
    }

    public void PickUp(Transform playerHead)
    {
        // Set phisics, original parent, set player's head as parent
        rb.isKinematic = true;
        originalParent = transform.parent;
        transform.SetParent(playerHead);
        helmetParent = playerHead;

        // Align the helmet with the player's head
        transform.localPosition = helmetPos;
        transform.localRotation = Quaternion.identity;

        isPickedUp = true;
    }

    public void Drop()
    {
        // Enable physics on the helmet, consider players velocity, detach helmet
        rb.isKinematic = false;

        parentScript = FindPlayerControllerInParents(transform);
        if (parentScript != null)
            rb.velocity = parentScript.gameObject.GetComponent<CharacterController>().velocity / 1.9f;

        transform.SetParent(null);
        helmetParent = null;

        // AddForce and Torque
        rb.AddForce(transform.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(transform.up * dropUpwardForce, ForceMode.Impulse);
        Vector3 torque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        rb.AddTorque(torque.normalized * dropTorque);

        isPickedUp = false;
    }

    public bool ActivateSuperState()
    {
        // Scale up and set player's helmet speed
        StartCoroutine(LerpScaleAndPositionCoroutine(
            new Vector3(activationScaleMultiplyer, activationScaleMultiplyer, activationScaleMultiplyer),
            Vector3.zero,
            activationScaleSpeed));

        parentScript = FindPlayerControllerInParents(transform);
        SoundFXManager.instance.PlaySoundFXClip(helmetActivateSounds, transform, 1f);

        if (parentScript != null)
        {
            parentScript.playerSpeed *= activationPlayerSpeedMultiplyer;
            superStateActivated = true;
            return true;
        }
        else
        {
            Debug.LogWarning("Parent script failed to assign");
            return false;
        }
    }

    public bool DeactivateSuperState(bool adjustPosition = true)
    {
        // scale down, set player's speed to default
        StartCoroutine(LerpScaleAndPositionCoroutine(
            Vector3.one * initialScaleMultiplyer,
            helmetPos + transform.localPosition,
            activationScaleSpeed,
            adjustPosition));

        parentScript = FindPlayerControllerInParents(transform);
        parentScript.playerSpeed = parentScript.initialPlayerSpeed;
        superStateActivated = false;

        // play sound
        if (helmetDeactivateSounds.Length > 0)
            SoundFXManager.instance.PlaySoundFXClip(helmetDeactivateSounds, transform, 1f);
        else
            SoundFXManager.instance.PlaySoundFXClip(helmetActivateSounds, transform, 1f, 1.2f);

        return false;
    }

    public PlayerController FindPlayerControllerInParents(Transform currentTransform)
    {
        // Find player in hierarchy
        Transform parentTransform = currentTransform.parent;
        while (parentTransform != null)
        {
            PlayerController parentScript = parentTransform.GetComponent<PlayerController>();
            if (parentScript != null)
            {
                return parentScript;
            }
            parentTransform = parentTransform.parent;
        }
        return null;
    }

    public virtual void SuperAbility()
    {
        Debug.LogWarning("Super ability not assigned!");
    }

    public virtual void SuperAttack()
    {
        Debug.LogWarning("Super attack not assigned!");
    }

    private IEnumerator LerpScaleAndPositionCoroutine(
        Vector3 targetScale,
        Vector3 targetPosition,
        float speed,
        bool lerpPosition = true,
        float frequency = 1f,
        float amplitude = 0.25f)
    {
        Vector3 initialScale = transform.localScale;
        Vector3 initialPosition = transform.localPosition;
        float t = 0f;

        if (!lerpPosition)
            transform.localPosition = helmetPos;

        // smooth scale and position, add sine wave for jelly-like interpolation effect for scale, and apply all 
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            float easedTScale = Mathf.SmoothStep(0f, 1f, t);
            float easedTPosition = 1f - Mathf.Pow(1f - t, 3f);
            float scaleMultiplier = 1f + Mathf.Sin(easedTScale * Mathf.PI * frequency) * amplitude;

            transform.localScale = Vector3.Lerp(initialScale, targetScale * scaleMultiplier, easedTScale);
            if (lerpPosition)
                transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, easedTPosition);
            yield return null;
        }

        // Double check scale and position
        transform.localScale = targetScale;
        if (lerpPosition) transform.localPosition = targetPosition;
    }

    public IEnumerator RunSuperAttackCooldown(float cooldownTime)
    {
        canSuperAttack = false;
        yield return new WaitForSeconds(superAttackCooldown);
        canSuperAttack = true;
    }

    public IEnumerator RunSuperAbilityCooldown(float cooldownTime)
    {
        canSuperAbility = false;
        yield return new WaitForSeconds(superAbilityCooldown);
        canSuperAbility = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.CompareTag("DeadlyWeapon") && other.transform.parent != transform.parent.parent)
        {
            if (helmetParent != null)
            {
                parentScript = FindPlayerControllerInParents(transform);
                parentScript.PickUp();
            }
        }
    }

}
