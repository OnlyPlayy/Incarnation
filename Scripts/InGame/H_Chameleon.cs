using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Helmet Chameleon
public class H_Chameleon : Helmet
{
    private bool imprintSpawnTurnLeft = false;

    [Header("Chameleon settings")]
    // imprint
    public GameObject imprintPrefab;
    [SerializeField] private float imprintSpawnInterval = 0.1f;

    // ability
    [Space(5)]
    public GameObject chameleonVisible;
    public GameObject chameleonHidden;
    public float targetAlpha = 0.04f;
    public float changingTime = 1.0f;
    private Renderer hiddenHelmetRenderer;

    // attack
    [Space(5)]
    public float chokeTime = 0.2f;
    private TongueSystem tongueSystem;


    void Awake()
    {
        tongueSystem = GetComponent<TongueSystem>();
        hiddenHelmetRenderer = chameleonHidden.GetComponent<Renderer>();
        InvokeRepeating("ImprintSpawnIntervalInitiator", 0f, imprintSpawnInterval);
    }

    void Update()
    {
        if (hidden && !superStateActivated)
            Appear();
    }

    public override void SuperAttack()
    {
        if (canSuperAttack)
        {
            StartCoroutine(TongueSystemInitiator());
            StartCoroutine(RunSuperAttackCooldown(superAttackCooldown));
            Appear();
        }
    }

    public override void SuperAbility()
    {
        if (canSuperAbility)
        {
            if (!hidden)
                StartCoroutine(Disappear());
            else
                Appear();

            StartCoroutine(RunSuperAbilityCooldown(superAbilityCooldown));
        }
    }

    private IEnumerator Disappear()
    {
        SoundFXManager.instance.PlaySoundFXClip(abilityActivateSounds, transform, 1f);

        ReplaceHelmetWith(chameleonVisible, chameleonHidden);
        hidden = true;
        yield return null;

        // get all materials
        Material[] materials = hiddenHelmetRenderer.materials;

        // Store original colors and calculate target colors for all materials
        Color[] originalColors = new Color[materials.Length];
        Color[] targetColors = new Color[materials.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
            targetColors[i] = new Color(originalColors[i].r, originalColors[i].g, originalColors[i].b, targetAlpha);
        }

        float elapsedTime = 0f;

        while (elapsedTime < changingTime)
        {
            // Interpolate colors for all materials
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].color = Color.Lerp(originalColors[i], targetColors[i], elapsedTime / changingTime);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure materials fully fade out
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = targetColors[i];
        }
    }

    private void Appear()
    {
        SoundFXManager.instance.PlaySoundFXClip(abilityDeactivateSounds, transform, 1f);

        // Revert alpha color change
        Material[] materials = hiddenHelmetRenderer.materials;

        Color[] originalColors = new Color[materials.Length];
        Color[] targetColors = new Color[materials.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
            targetColors[i] = new Color(originalColors[i].r, originalColors[i].g, originalColors[i].b, 1);
        }

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = targetColors[i];
        }

        // Replace helmet
        ReplaceHelmetWith(chameleonHidden, chameleonVisible);
        hidden = false;
    }

    private void ReplaceHelmetWith(GameObject hideObject, GameObject appearObject)
    {
        hideObject.SetActive(false);
        appearObject.SetActive(true);
    }

    private void ImprintSpawnIntervalInitiator()
    {
        if (hidden)
        {
            if (!imprintSpawnTurnLeft)
                SpawnImprint(true);
            else
                SpawnImprint(false);

            imprintSpawnTurnLeft = !imprintSpawnTurnLeft;
        }
    }

    private void SpawnImprint(bool left)
    {
        float legOffset = 0.3f;
        float playerMovement = parentScript.playerMovement.magnitude;
        Material imprintMaterial = imprintPrefab.GetComponent<Renderer>().sharedMaterial;
        Color originalColor = imprintMaterial.color;

        Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, playerMovement);

        // Switch between left and right
        Vector3 localSpawnPosition = new Vector3(left ? -legOffset : legOffset, 0f, 0f);
        Quaternion localSpawnRotation = Quaternion.Euler(0f, 180.0f, 0f);

        // Transform local position and rotation to global
        Vector3 spawnPosition = transform.TransformPoint(localSpawnPosition);
        spawnPosition.y = 0.01f;
        Quaternion spawnRotation = transform.rotation * localSpawnRotation;

        GameObject imageInstance = Instantiate(imprintPrefab, spawnPosition, spawnRotation);
        imageInstance.GetComponent<Renderer>().material.color = newColor;

        Destroy(imageInstance, 1.0f);
    }

    private IEnumerator TongueSystemInitiator()
    {
        SoundFXManager.instance.PlaySoundFXClip(attackSounds, transform, 1f);

        // spawn tongue prefab
        Vector3 forwardPosition = transform.position + transform.forward;
        GameObject spawnedObject = Instantiate(tongueSystem.attackTonguePrefab, forwardPosition, transform.rotation * Quaternion.Euler(90f, 0f, 0f));
        spawnedObject.transform.parent = transform.parent.parent;
        float tt = tongueSystem.tongueThickness;
        spawnedObject.transform.localScale = new Vector3(tt, 1f, tt);

        // wait and destroy preafab
        yield return new WaitForSeconds(chokeTime);
        Destroy(spawnedObject, tongueSystem.tongueScalingTime/3);

        // run proper super attack
        tongueSystem.LocatePlayers();
    }
}

