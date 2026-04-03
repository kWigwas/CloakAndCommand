using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Optional: disables the Continue button when there is no layout save. Put on the same object as <see cref="SceneNavigator"/> (e.g. main menu camera).
/// Looks for a button named <c>ContinueButton</c>, or assign <see cref="continueButton"/> explicitly.
/// </summary>
public class MainMenuButtons : MonoBehaviour
{
    [SerializeField] SceneNavigator navigator;
    [Tooltip("If null, searches for a Button on a GameObject named ContinueButton.")]
    [SerializeField] Button continueButton;

    void Awake()
    {
        if (navigator == null)
            navigator = GetComponent<SceneNavigator>();
    }

    void Start()
    {
        if (navigator == null)
            return;

        Button btn = continueButton;
        if (btn == null)
        {
            var go = GameObject.Find("ContinueButton");
            if (go != null)
                btn = go.GetComponent<Button>();
        }

        if (btn != null)
            btn.interactable = navigator.HasSaveToContinue();
    }
}
