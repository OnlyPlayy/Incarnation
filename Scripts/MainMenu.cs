using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settings;

    [Header("Buttons")]
    [SerializeField] private Slider openSettings;
    [SerializeField] private Button closeSettings;

    private void Start()
    {
        if (PlayerConfigurationManager.Instance != null)
            PlayerConfigurationManager.Instance.KillConfigs();


        MusicManager.instance.PlayRandomIfOutsideOfScope();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("PlayerSetup");
    }

    public void OpenCredits()
    {
        SceneManager.LoadScene("CreditsScreen");
    }

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settings.SetActive(true);
        openSettings.Select();
    }

    public void CloseSettings()
    {
        settings.SetActive(false);
        mainMenu.SetActive(true);
        closeSettings.Select();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
