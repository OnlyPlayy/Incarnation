using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public static bool GameIsReallyPaused = false;

    public GameObject pauseMenuUI;
    private static Button[] buttons;
    private static int selectedIndex = 0;

    void Start()
    {
        buttons = pauseMenuUI.GetComponentsInChildren<Button>();

        if (buttons.Length > 0)
        {
            SelectButton(selectedIndex);
        }
    }

    void Update()
    {
        PauseListener();
    }

    void PauseListener()
    {
        if (GameIsPaused != GameIsReallyPaused)
        {
            GamePause(GameIsPaused);
        }
    }

    public void GamePause(bool pause)
    {
        pauseMenuUI.SetActive(pause);
        Time.timeScale = pause ? 0f : 1f;
        GameIsReallyPaused = pause;
        GameIsPaused = pause;

        if (pause)
        {
            SelectButton(selectedIndex);
        }
    }

    public void LoadMainMenu()
    {
        GamePause(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public static void NavigationButtons(Vector2 controlDir)
    {
        if (controlDir.y < -0.5f)
        {
            SelectButton(selectedIndex + 1);
        }
        else if (controlDir.y > 0.5f)
        {
            SelectButton(selectedIndex - 1);
        }
    }

    public static void ExecuteSelectedButton()
    {
        if (buttons != null && selectedIndex >= 0 && selectedIndex < buttons.Length)
        {
            buttons[selectedIndex].onClick.Invoke();
        }
    }

    private static void SelectButton(int index)
    {
        index = Mathf.Clamp(index, 0, buttons.Length - 1);

        if (buttons.Length > 0)
        {
            buttons[selectedIndex].interactable = true;
            selectedIndex = index;
            buttons[selectedIndex].Select();
            buttons[selectedIndex].interactable = false;
        }
    }
}
