using System;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    public static bool isPaused = false;

    /// <summary>Raised when the pause menu opens (after <see cref="Time.timeScale"/> is set to 0).</summary>
    public static event Action GamePaused;

    void Update()
    {
        if (PlayerControls.Instance == null)
            return;

        if (PlayerControls.Instance.pausePressed)
        //if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Start()
    {
        Resume();
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        isPaused = true;
        Time.timeScale = 0;
        GamePaused?.Invoke();
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
    }
}
