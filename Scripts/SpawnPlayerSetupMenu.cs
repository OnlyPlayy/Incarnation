using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class SpawnPlayerSetupMenu : MonoBehaviour
{
    public static SpawnPlayerSetupMenu instance;

    public GameObject playerSetupMenuPrefab;
    public PlayerInput input;


    [System.NonSerialized] public InputSystemUIInputModule selfComponent;

    private void Awake()
    {
        if (instance != null) instance = this;
        var rootMenu = GameObject.Find("MainLayout");
        if (rootMenu != null)
        {
            var menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform);
            selfComponent = menu.GetComponentInChildren<InputSystemUIInputModule>();
            input.uiInputModule = selfComponent; // sets wrong button in some scenarios
            menu.GetComponent<PlayerSetupMenuController>().SetPlayerIndex(input.playerIndex);
            menu.GetComponent<PlayerSetupMenuController>().owningInput = input;
            menu.GetComponent<PlayerSetupMenuController>().menagerObject = gameObject;
            menu.GetComponent<PlayerSetupMenuController>().selfInstance = menu;
        }
        SetJoinPromptAsLastObject();
        TakeDownBots();
    }

    public void SetJoinPromptAsLastObject()
    {
        // find main layout
        var mainLayout = GameObject.Find("MainLayout");
        if (mainLayout == null)
            return;

        // find join inside main layout
        var joinPrompt = mainLayout.transform.Find("Join_PlayerSetupMenu");
        if (joinPrompt == null)
            return;

        // set Join prompt as last object
        joinPrompt.SetParent(transform);
        joinPrompt.SetParent(mainLayout.transform);
    }

    public void TakeDownBots()
    {
        var mainLayout = GameObject.Find("MainLayout");
        if (mainLayout == null)
            return;

        if(mainLayout.transform.childCount > 4)
        {
            Transform[] botUIChildren = mainLayout.GetComponentsInChildren<Transform>().Where(child => child.name == "Bot_SetupMenu(Clone)").ToArray();
            PlayerConfigurationManager.Instance.HandleRemoveBotUI();
            TakeDownBots();
        }
    }
}
