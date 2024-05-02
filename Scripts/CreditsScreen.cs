using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScreen : MonoBehaviour
{
    [SerializeField] private AudioClip creditsSong;

    void Start()
    {
        MusicManager.instance.PlaySpecifiedClip(creditsSong);
        StartCoroutine(ExitCreditsScreen(75f));
    }

    IEnumerator ExitCreditsScreen(float waitSeconds)
    {
        yield return new WaitForSeconds(waitSeconds);
        LoadMainMenu();
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
