using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerSetupMenuController : MonoBehaviour
{
    private int PlayerIndex;

    public TextMeshProUGUI titleText;
    [SerializeField] private GameObject readyPanel;
    [SerializeField] public GameObject menuPanel;
    [SerializeField] private Button readyButton;
    public MultiplayerEventSystem eventSystem;
    public Button stupidColor;

    // lobby unit and input
    [NonSerialized] public PlayerInput owningInput;
    [NonSerialized] public GameObject menagerObject;
    [NonSerialized] public GameObject selfInstance;

    private int caseScenario = 0;


    private void Start()
    {
        if (owningInput == null)
        {
            Debug.LogWarning("PlayerInput component not found on " + gameObject.name);
            return;
        }
        else
        {
            // Subscribe to the cancel action
            owningInput.actions.FindAction("PlayerUI/Cancel").performed += ctx => CancelAction();
            Debug.Log("PlayerInput component found!");

            owningInput.actions.FindAction("PlayerUI/Move").performed += ctx => MoveAction(ctx);
        }
    }

    // SET PLAYER INDEX
    public void SetPlayerIndex(int pi)
    {
        PlayerIndex = pi;
        titleText.SetText("Player " + (pi + 1).ToString());
    }
    public void UnSetPlayerIndex()
    {
        if (PlayerConfigurationManager.Instance == null) return;

        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs();
        if (playerConfigs != null)
        {
            if (playerConfigs.Count() > 1)
                SceneManager.LoadScene("PlayerSetup");
            else
                SceneManager.LoadScene("MainMenu");
        }
        PlayerConfigurationManager.Instance.KillConfigs();
    }


    // SET COLOR
    public void SetColor(Material color)
    {
        caseScenario++;
        PlayerConfigurationManager.Instance.SetPlayerColor(PlayerIndex, color);
        readyPanel.SetActive(true);
        readyButton.Select();
        menuPanel.SetActive(false);
        titleText.color = color.color;
        Debug.Log("Sets color to: " + color.name);
    }

    private void UnsetColor()
    {
        caseScenario--;
        readyPanel.SetActive(false);
        menuPanel.SetActive(true);
        stupidColor.Select();
        Debug.Log("Unsetting color");
    }


    // READY PLAYER
    public void ReadyPlayer()
    {
        caseScenario++;
        PlayerConfigurationManager.Instance.ReadyPlayer(PlayerIndex);
        readyButton.gameObject.SetActive(false);
    }

    private void UnReadyPlayer()
    {
        if (PlayerConfigurationManager.Instance == null) return;
        PlayerConfigurationManager.Instance.UnReadyPlayer(PlayerIndex);
        readyButton.gameObject.SetActive(true);
    }


    // CANCEL ACTION
    public void CancelAction()
    {
        switch (caseScenario)
        {
            //case 1:
            //    UnsetColor();
            //    break;
            case 2:
                UnReadyPlayer();
                break;
            default:
                UnSetPlayerIndex();
                break;
        }
    }

    private void MoveAction(InputAction.CallbackContext ctx)
    {
        Vector2 moveValue = ctx.ReadValue<Vector2>();
        if (PlayerConfigurationManager.Instance == null) return;
        if (moveValue.x > 0.8f)
        {
            PlayerConfigurationManager.Instance.HandleAddBotUI();
        }
        else if (moveValue.x < -0.8f)
        {
            PlayerConfigurationManager.Instance.HandleRemoveBotUI();
        }
        moveValue.x = 0f;
    }
}

