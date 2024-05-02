using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    [Header("HUD and score board")]
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject playerScoreHUDUnit;
    [NonSerialized] public GameObject gameHUD;

    [Header("Game wrap")]
    [SerializeField] private GameObject cupPrefab;


    void Start()
    {
        Instance = this;
        gameHUD = Instantiate(HUD);
        UpdatePlayersScoreBoard();
    }

    public void UpdatePlayersScoreBoard()
    {
        var playerConfigsCopy = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToList();

        Transform[] foundObjects = gameHUD.GetComponentsInChildren<Transform>().Where(child => child.name == "PlayerScoreHUDUnit(Clone)").ToArray();
        foreach (Transform obj in foundObjects)
        {
            Destroy(obj.gameObject);
        }

        foreach (var playerConfig in playerConfigsCopy)
        {
            var playerScoreHUD = Instantiate(playerScoreHUDUnit);
            playerScoreHUD.transform.SetParent(gameHUD.transform.GetChild(0), false);

            // set board background
            var playerBoardImage = playerScoreHUD.transform.GetChild(0).GetComponent<Image>();
            Color playerColor = playerConfig.PlayerColor;
            playerColor.a = 0.6f;
            playerBoardImage.color = playerColor;

            // set player number
            var playerNoText = playerScoreHUD.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            if (!playerConfig.IsBot)
                playerNoText.text = "Player " + (playerConfig.PlayerIndex + 1);
            else
                playerNoText.text = "Bot " + (playerConfig.PlayerIndex + 1);

            // set player score
            var playerScoreText = playerScoreHUD.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            playerScoreText.text = "" + playerConfig.Score;
        }
    }

    public static void ShowWrapRoundUI(Color color)
    {
        var sceneHUD = GameObject.Find("HUD(Clone)");
        if (sceneHUD != null)
        {
            // find the score object and change its colour
            var wrapRoundUI = sceneHUD.transform.Find("WrapRoundUI");
            if (wrapRoundUI != null)
            {
                wrapRoundUI.gameObject.SetActive(true);
                wrapRoundUI.GetComponentInChildren<TextMeshProUGUI>().color = color;
            }
        }
    }

    public static void ShowWrapGameUI(Color color, int playerIndex)
    {
        var sceneHUD = GameObject.Find("HUD(Clone)");
        if (sceneHUD != null)
        {
            // find the score object and change its colour
            var wrapRoundUI = sceneHUD.transform.Find("WrapGameUI");
            if (wrapRoundUI != null)
            {
                wrapRoundUI.gameObject.SetActive(true);
                wrapRoundUI.GetComponentInChildren<TextMeshProUGUI>().color = color;
                if (!PlayerConfigurationManager.Instance.GetPlayerConfigs()[playerIndex].IsBot)
                    wrapRoundUI.GetComponentInChildren<TextMeshProUGUI>().text = "Player " + (playerIndex + 1) + " WINS!";
                else
                    wrapRoundUI.GetComponentInChildren<TextMeshProUGUI>().text = "Bot " + (playerIndex + 1) + " WINS!";
                Instance.UpdatePlayersScoreBoard();
            }
        }
    }

    public void WrapGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        FindSpaceForTheCup(player.transform.position);
    }

    void FindSpaceForTheCup(Vector3 playerPosition)
    {
        float groundCheckOffset = 5f;

        // Perform raycasts around the player
        if (CheckGround(playerPosition + Vector3.left * groundCheckOffset))
            return;
        if (CheckGround(playerPosition + Vector3.right * groundCheckOffset))
            return;
        if (CheckGround(playerPosition + Vector3.forward * groundCheckOffset))
            return;
        if (CheckGround(playerPosition + Vector3.back * groundCheckOffset))
            return;

        Instantiate(cupPrefab, playerPosition, Quaternion.Euler(0, 90f, 0));
    }

    private bool CheckGround(Vector3 rayPosition)
    {
        float raycastDistance = 5f;

        // Perform a raycast downward to check for ground
        RaycastHit hit;
        if (Physics.Raycast(rayPosition, Vector3.down, out hit, raycastDistance))
        {
            // Spawn cup prefab at the position where ground is hit
            if (hit.collider.CompareTag("Ground"))
            {
                Instantiate(cupPrefab, hit.point, Quaternion.Euler(0, 90f, 0));
                return true;
            }
        }
        return false;
    }
}
