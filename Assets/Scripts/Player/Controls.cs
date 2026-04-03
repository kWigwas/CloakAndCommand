using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public static PlayerControls Instance { get; private set; }

    /// <summary>
    /// Text shown by <see cref="ControlLabel"/> (and anything else that calls <see cref="TryGetControlDisplayName"/>).
    /// Keys must match the string field names on this class — use <c>nameof(...)</c> so renames stay in sync.
    /// Edit this dictionary in code only.
    /// </summary>
    static readonly Dictionary<string, string> ControlDisplayNames = new Dictionary<string, string>
    {
        { nameof(horizontalAxis), "Move" },
        { nameof(verticalAxis), "Move" },
        { nameof(jumpButton), "Jump" },
        { nameof(fire1Button), "Primary" },
        { nameof(fire2Button), "Secondary" },
        { nameof(fire3Button), "Ability" },
        { nameof(rollButton), "Roll" },
        { nameof(interactButton), "Use" },
        { nameof(pauseButton), "Pause" },
    };

    /// <summary>
    /// How each Input Manager button string should look in UI (what <see cref="ControlLabel.GetAssignedKey"/> shows).
    /// Keys must match Project Settings → Input Manager (positive/negative button), case-insensitive.
    /// </summary>
    static readonly Dictionary<string, string> ControlKeyDisplayOverrides =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "left shift", "LSHIFT" },
            { "right shift", "RSHIFT" },
            { "left ctrl", "LCTRL" },
            { "right ctrl", "RCTRL" },
            { "left alt", "LALT" },
            { "right alt", "RALT" },
            { "space", "SPACE" },
            { "return", "ENTER" },
            { "keypad enter", "NUMPAD ENTER" },
            { "escape", "ESC" },
            { "tab", "TAB" },
            { "backspace", "BACKSPACE" },
            { "a", "A" },
            { "b", "B" },
            { "c", "C" },
            { "d", "D" },
            { "e", "E" },
            { "f", "F" },
            { "g", "G" },
            { "h", "H" },
            { "i", "I" },
            { "j", "J" },
            { "k", "K" },
            { "l", "L" },
            { "m", "M" },
            { "n", "N" },
            { "o", "O" },
            { "p", "P" },
            { "q", "R" },
            { "r", "S" },
            { "s", "T" },
            { "t", "U" },
            { "u", "V" },
            { "v", "V" },
            { "w", "W" },
            { "x", "Y" },
            { "y", "X" },
            { "z", "Z" }
        };

    /// <summary>
    /// Applies <see cref="ControlKeyDisplayOverrides"/> to one key or to combined <c>a / d</c> style strings.
    /// </summary>
    public static string FormatBindingKeyString(string rawFromInputManager)
    {
        if (string.IsNullOrWhiteSpace(rawFromInputManager))
            return rawFromInputManager;

        rawFromInputManager = rawFromInputManager.Trim();
        const string sep = " / ";
        if (rawFromInputManager.IndexOf(sep, StringComparison.Ordinal) < 0)
        {
            return ControlKeyDisplayOverrides.TryGetValue(rawFromInputManager, out string one)
                ? one
                : rawFromInputManager;
        }

        string[] parts = rawFromInputManager.Split(new[] { sep }, StringSplitOptions.None);
        for (int i = 0; i < parts.Length; i++)
        {
            string p = parts[i].Trim();
            parts[i] = ControlKeyDisplayOverrides.TryGetValue(p, out string o) ? o : p;
        }

        return string.Join(sep, parts);
    }

    [Header("Input Names")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string jumpButton = "Jump";
    public string fire1Button = "Fire1";
    public string fire2Button = "Fire2";
    public string fire3Button = "Fire3";
    public string rollButton = "Fire2";
    public string interactButton = "Interact";
    public string pauseButton = "Pause";

    [Header("Current Input State")]

    public float horizontalInput;
    public float verticalInput;
    public bool jumpPressed;
    public bool fire1Pressed;
    public bool fire2Pressed;
    public bool fire3Pressed;
    public bool rollPressed;
    public bool interactPressed;
    public bool pausePressed;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Looks up <see cref="ControlDisplayNames"/>.</summary>
    public bool TryGetControlDisplayName(string bindingFieldName, out string displayName)
    {
        if (!string.IsNullOrEmpty(bindingFieldName) &&
            ControlDisplayNames.TryGetValue(bindingFieldName, out displayName) &&
            !string.IsNullOrEmpty(displayName))
            return true;

        displayName = null;
        return false;
    }

    void Update()
    {
        // Read all inputs once per frame
        horizontalInput = Input.GetAxisRaw(horizontalAxis);
        verticalInput = Input.GetAxisRaw(verticalAxis);
        jumpPressed = Input.GetButtonDown(jumpButton);
        fire1Pressed = Input.GetButtonDown(fire1Button);
        fire2Pressed = Input.GetButtonDown(fire2Button);
        fire3Pressed = Input.GetButtonDown(fire3Button);
        rollPressed = Input.GetButtonDown(rollButton);
        interactPressed = Input.GetButtonDown(interactButton);
        pausePressed = Input.GetButtonDown(pauseButton);
    }
}
