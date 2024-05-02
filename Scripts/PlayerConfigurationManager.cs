using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : MonoBehaviour
{
    private List<PlayerConfiguration> playerConfigs;

    [SerializeField] private Material[] PlayerMaterials;
    [SerializeField] private int MaxPlayers = 4;
    [SerializeField] private string[] mapScenes;
    [SerializeField] private float loadingNextSceneWaitTime = 1f;
    [SerializeField] private AudioClip[] pointScoredSound;
    private int scoreWinCondition = 10;
    private bool lastRound = false;

    [SerializeField] private GameObject botSetupMenu;
 
    public static PlayerConfigurationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("SINGLETON - Trying to create another instance of singleton!");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            playerConfigs = new List<PlayerConfiguration>();
        }
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void KillConfigs()
    {
        playerConfigs = null;
        Instance = null;
        Destroy(gameObject);
        Destroy(Instance);
        Destroy(this);
    }
    private bool EverybodyReady()
    {
        if (playerConfigs.Count >= 1 && playerConfigs.All(p => p.IsReady == true))
        { 
            return true; 
        }
        else
            return false;

    }

    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    public void SetPlayerColor(int index, Material color)
    {
        playerConfigs[index].PlayerMaterial = color;
        playerConfigs[index].PlayerColor = color.color;
    }

    public void ReadyPlayer(int index)
    {
        playerConfigs[index].IsReady = true;
        if (EverybodyReady())
        {
            StartCoroutine(LoadNextMap("Level1"));
        }
    }

    public void UnReadyPlayer(int index)
    {
        playerConfigs[index].IsReady = false;
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        Debug.Log("Player joined " + pi.playerIndex);

        // Check if the player index already exists in playerConfigs
        if (!playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
        {
            pi.transform.SetParent(transform);
            playerConfigs.Add(new PlayerConfiguration(pi, playerConfigs.Count));
        }

        // Hide join prompt if maximum players reached
        if (playerConfigs.Count >= MaxPlayers)
        {
            TakeDownJoinPrompt();
        }
    }

    public void AddBot()
    {
        // Create a bot configuration with a random material not used by any player
        PlayerConfiguration botConfig = new PlayerConfiguration(null, playerConfigs.Count);
        Material randomMaterial = GetRandomUnusedMaterial();
        botConfig.PlayerMaterial = randomMaterial;
        botConfig.PlayerColor = randomMaterial.color;
        botConfig.IsBot = true;
        botConfig.IsReady = true;
        playerConfigs.Add(botConfig);
    }

    public void HandleAddBotUI()
    {
        var rootMenu = GameObject.Find("MainLayout");
        if (rootMenu == null) return;

        if (rootMenu.transform.childCount < 5 && playerConfigs.Count < 4)
        {
            var botSetupMenuInstance = Instantiate(botSetupMenu, rootMenu.transform);
        }

        // find join inside main layout
        var joinPrompt = rootMenu.transform.Find("Join_PlayerSetupMenu");
        if (joinPrompt == null)
            return;

        // set Join prompt as last object
        joinPrompt.SetParent(transform);
        joinPrompt.SetParent(rootMenu.transform);
    }

    private Material GetRandomUnusedMaterial()
    {
        // Filter out materials that are already used by players
        List<Material> unusedMaterials = PlayerMaterials
            .Where(material => !playerConfigs.Any(pc => pc.PlayerMaterial == material))
            .ToList();

        // If there are unused materials, return a random one; otherwise, return null
        if (unusedMaterials.Count > 0)
        {
            return unusedMaterials[Random.Range(0, unusedMaterials.Count)];
        }
        else
        {
            Debug.LogWarning("No unused materials available for bot.");
            return null;
        }
    }

    public void RemoveBot()
    {
        PlayerConfiguration botToRemove = playerConfigs.FirstOrDefault(pc => pc.IsBot == true);
        if (botToRemove != null)
        {
            playerConfigs.Remove(botToRemove);
        }
    }

    public void HandleRemoveBotUI()
    {
        GameObject mainLayout = GameObject.Find("MainLayout");
        if (mainLayout != null)
        {
            Transform[] botUIChildren = mainLayout.GetComponentsInChildren<Transform>().Where(child => child.name == "Bot_SetupMenu(Clone)").ToArray();

            // delete the first bot setupmenu
            if (botUIChildren.Length > 0)
            {
                Destroy(botUIChildren[0].gameObject);
            }
        }
    }


    private void TakeDownJoinPrompt()
    {
        var mainLayout = GameObject.Find("MainLayout");
        if (mainLayout == null)
            return;

        var joinPrompt = mainLayout.transform.Find("Join_PlayerSetupMenu");
        if (joinPrompt == null)
            return;

        joinPrompt.gameObject.SetActive(false);
    }

    public void AwakeNextMapLoad(string nextMapName = "")
    {
        StartCoroutine(LoadNextMap(nextMapName));
    }

    private IEnumerator LoadNextMap(string nextMapName = "")
    {
        yield return new WaitForSeconds(loadingNextSceneWaitTime);

        // Don't execute further if not everybody is ready
        if (!EverybodyReady())
            yield break;

        // Check how many players and bots joined, add Bot if only one player
        GameObject mainLayout = GameObject.Find("MainLayout");
        if (mainLayout != null)
        {
            if (mainLayout.transform.childCount < 3) HandleAddBotUI();
        }

        // Add Bots at the beginning of the game
        if (mainLayout != null && SceneManager.GetActiveScene().name == "PlayerSetup")
        {
            Transform[] botUIChildren = mainLayout.GetComponentsInChildren<Transform>().Where(child => child.name == "Bot_SetupMenu(Clone)").ToArray();
            for (int i = 0;  i < botUIChildren.Length; i++)
            {
                if (playerConfigs.Count < 4)
                AddBot();
            }
        }
        if (playerConfigs.Count < 2)
        {
            AddBot();
        }
        if (string.IsNullOrEmpty(nextMapName) || !mapScenes.Contains(nextMapName) && nextMapName != "MainMenu")
        {
            // Load a random map
            int randomIndex = Random.Range(0, mapScenes.Length);
            SceneManager.LoadScene(mapScenes[randomIndex]);
        }
        else
        {
            // Load the map by name
            SceneManager.LoadScene(nextMapName);
        }
    }

    // SCORE MANAGEMENT AND GAME LOGIC
    public void CheckAndUpdatePlayerScore()
    {
        // Count the number of players alive
        int alivePlayersCount = playerConfigs.Count(p => p.Alive);
        Debug.Log("alivePlayersCount: " + alivePlayersCount);

        // If only one player is alive
        if (alivePlayersCount == 1)
        {
            int alivePlayerIndex = playerConfigs.FindIndex(p => p.Alive);

            UpdatePlayerScore(alivePlayerIndex, 1);

            PlayerConfiguration alivePlayer = playerConfigs[alivePlayerIndex];
            bool scoreAboveRequired = alivePlayer.Score >= scoreWinCondition;

            // Load the next map
            if (scoreAboveRequired)
            {
                WrapUpTheGame();
                GameplayManager.ShowWrapGameUI(alivePlayer.PlayerColor, alivePlayer.PlayerIndex);
                lastRound = true;
            }
            else
                StartCoroutine(LoadNextMap());
        }
        if (alivePlayersCount == 0 && lastRound)
        {
            AwakeNextMapLoad("MainMenu");
        }
        MultipleTargetCamera.UpdateAssignedPlayers();
    }

    public void WrapUpTheGame()
    {
        GameplayManager.Instance.WrapGame();
    }

    // SCORE
    public void UpdatePlayerScore(int playerIndex, int score)
    {
        // Find the player configuration by player index
        PlayerConfiguration player = playerConfigs.FirstOrDefault(p => p.PlayerIndex == playerIndex);
        if (player != null)
        {
            // Update the player's score
            player.Score += score;
            GameplayManager.ShowWrapRoundUI(player.PlayerColor);
            StartCoroutine(PlayPointScoredSound());
        }
    }

    IEnumerator PlayPointScoredSound()
    {
        yield return new WaitForSeconds(0.4f);
        SoundFXManager.instance.PlaySoundFXClip(pointScoredSound, transform, 0.6f, 0.7f);
    }

    public int GetPlayerScore(int playerIndex)
    {
        // Find the player configuration by player index
        PlayerConfiguration player = playerConfigs.FirstOrDefault(p => p.PlayerIndex == playerIndex);
        if (player != null)
        {
            return player.Score;
        }
        // If player not found, return 0
        Debug.Log("Player not found");
        return 0;
    }
}

public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput pi, int index)
    {
        if (pi != null)
        {
            PlayerIndex = pi.playerIndex;
            Input = pi;
        }
        else
        {
            PlayerIndex = index;
        }
    }

    public PlayerInput Input { get; set; }
    public int PlayerIndex { get; set; }
    public bool IsReady { get; set; }
    public Material PlayerMaterial { get; set; }
    public Color PlayerColor { get; set; }
    public int Score { get; set; }
    public GameObject Helmet { get; set; }
    public bool Alive { get; set; }
    public int WhispCount { get; set; }
    public bool IsBot { get; set; }
}

