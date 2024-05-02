using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject botPrefab;

    [Header("Whisp")]
    [SerializeField] private GameObject whispPrefab;
    private float detectionRadius = 10f;
    private float minSpawnInterval = 5f;
    private float maxSpawnInterval = 15f;

    void Start()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();
        for (int i = 0; i < playerConfigs.Length; i++)
        {
            // Check if the player configuration has a valid color and material
            if (playerConfigs[i].PlayerMaterial != null && playerConfigs[i].PlayerColor != Color.clear)
            {
                bool isBot = playerConfigs[i].IsBot;
                var player = Instantiate(isBot? botPrefab : playerPrefab, playerSpawns[i].position, playerSpawns[i].rotation, gameObject.transform);
                if (!isBot) player.GetComponent<PlayerController>().InitializePlayer(playerConfigs[i]);
                else player.GetComponent<BotController>().InitializePlayer(playerConfigs[i]);
            }
            else
            {
                Debug.LogWarning("Player configuration at index " + i + " is not fully initialized and will not be instantiated.");
            }
        }
        MultipleTargetCamera.UpdateAssignedPlayers();
        StartCoroutine(SpawnWhispPeriodically());
    }
    IEnumerator SpawnWhispPeriodically()
    {
        while (true)
        {
            float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(spawnInterval);
            SpawnWhisp();
        }
    }

    void SpawnWhisp()
    {
        foreach (Transform spawnPoint in playerSpawns)
        {
            // check for players
            Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, detectionRadius);

            bool noPlayersNearby = true;
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    noPlayersNearby = false;
                    break;
                }
            }
            if (noPlayersNearby)
            {
                // spawn
                Instantiate(whispPrefab, spawnPoint.position, Quaternion.identity);
                break;
            }
        }
    }
}
