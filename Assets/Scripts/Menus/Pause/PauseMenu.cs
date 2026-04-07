using System;
using System.Collections;
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

    /// <summary>Yields until <see cref="isPaused"/> is false (uses unscaled frames so it works even if time scale is wrong).</summary>
    public static IEnumerator WaitWhilePaused()
    {
        while (isPaused)
            yield return null;
    }

    void OnDestroy()
    {
        // Single-scene loads destroy this object while globals may still say "paused" with timeScale 0.
        if (!isPaused)
            return;
        isPaused = false;
        Time.timeScale = 1f;
    }
}
