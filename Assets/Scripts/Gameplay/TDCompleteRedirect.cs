using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// On load (e.g. stealth hub): if <see cref="TDProgress.AllTDComplete"/>, loads the credits scene.
/// <see cref="TryOpenCreditsIfAllTdComplete"/> is also called from <see cref="TDEnemyCount"/> when the last stage is cleared,
/// because <see cref="SceneNavigator.GoBackToPreviousScene"/> often returns to MainMenu (empty history) instead of the hub,
/// which would skip this component entirely.
/// </summary>
[DisallowMultipleComponent]
public sealed class TDCompleteRedirect : MonoBehaviour
{
    const string PrefsRedirectDoneKey = "TD_AllComplete_CreditsRedirectOnce";

    public const string DefaultCreditsSceneName = "CreditsMenu";

    [Tooltip("Must match a scene in Build Settings.")]
    [SerializeField] string creditsSceneName = DefaultCreditsSceneName;

    [Tooltip("If true, auto-redirect to credits only the first time all TD levels are complete.")]
    [SerializeField] bool redirectOnlyOnce = true;

    /// <summary>
    /// If all four TD stages are marked complete, applies the same once-only prefs gate as the hub, then loads credits.
    /// </summary>
    /// <returns>True if a credits load was started (caller should not run other scene transitions).</returns>
    public static bool TryOpenCreditsIfAllTdComplete(string sceneName, bool redirectOnlyOnce)
    {
        if (!TDProgress.AllTDComplete || string.IsNullOrEmpty(sceneName))
            return false;

        if (redirectOnlyOnce && PlayerPrefs.GetInt(PrefsRedirectDoneKey, 0) != 0)
            return false;

        if (redirectOnlyOnce)
        {
            PlayerPrefs.SetInt(PrefsRedirectDoneKey, 1);
            PlayerPrefs.Save();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
        return true;
    }

    void Start()
    {
        TryOpenCreditsIfAllTdComplete(creditsSceneName, redirectOnlyOnce);
    }
}
