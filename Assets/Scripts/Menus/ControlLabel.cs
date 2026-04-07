using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Same GameObject as <see cref="TextMeshProUGUI"/>. Pick a <see cref="PlayerControls"/> binding; by default
/// shows only the key (after <see cref="PlayerControls.FormatBindingKeyString"/>), or action label only.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
[DisallowMultipleComponent]
public class ControlLabel : MonoBehaviour
{
    public enum TextFormat
    {
        Key,
        Label,
    }

    [Tooltip("PlayerControls string field name; set via dropdown below.")]
    [SerializeField] string bindingFieldName = nameof(PlayerControls.interactButton);

    [Tooltip("If null, uses PlayerControls.Instance when refreshing.")]
    [SerializeField] PlayerControls controlsOverride;

    [Tooltip("Key (default) = bound key only. Label = action name from PlayerControls, no key.")]
    [SerializeField] TextFormat textFormat = TextFormat.Key;

    TextMeshProUGUI _tmp;

    void Awake() => _tmp = GetComponent<TextMeshProUGUI>();

    void OnEnable() => Refresh();

    void Start() => Refresh();

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
            Refresh();
    }
#endif

    public static string[] GetPlayerControlsBindingFieldNames()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var list = new List<string>();
        foreach (FieldInfo fi in typeof(PlayerControls).GetFields(flags))
        {
            if (fi.IsStatic || fi.FieldType != typeof(string))
                continue;
            list.Add(fi.Name);
        }

        list.Sort(StringComparer.Ordinal);
        return list.ToArray();
    }

    public static string GetInputManagerNameFromControls(PlayerControls controls, string fieldName)
    {
        if (controls == null || string.IsNullOrEmpty(fieldName))
            return null;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo fi = typeof(PlayerControls).GetField(fieldName, flags);
        if (fi == null || fi.FieldType != typeof(string))
            return null;

        return (string)fi.GetValue(controls);
    }

    public void Refresh()
    {
        if (_tmp == null)
            _tmp = GetComponent<TextMeshProUGUI>();

        PlayerControls c = controlsOverride != null ? controlsOverride : PlayerControls.Instance;
        string inputName = GetInputManagerNameFromControls(c, bindingFieldName);
        if (string.IsNullOrEmpty(inputName))
        {
            _tmp.text = "—";
            return;
        }

        string key = GetAssignedKey(inputName);
        string label = ResolveDisplayLabel(c, bindingFieldName);

        _tmp.text = textFormat == TextFormat.Label ? label : key;
    }

    /// <summary>Template on <see cref="PlayerControls"/> if set, otherwise a simple title-case split of the field name.</summary>
    static string ResolveDisplayLabel(PlayerControls controls, string fieldName)
    {
        if (controls != null && controls.TryGetControlDisplayName(fieldName, out string fromTemplate) &&
            !string.IsNullOrEmpty(fromTemplate))
            return fromTemplate;
        return NicifyFieldName(fieldName);
    }

    static string NicifyFieldName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            return string.Empty;

        var sb = new StringBuilder();
        sb.Append(char.ToUpperInvariant(fieldName[0]));
        for (int i = 1; i < fieldName.Length; i++)
        {
            char ch = fieldName[i];
            if (char.IsUpper(ch))
                sb.Append(' ');
            sb.Append(ch);
        }

        return sb.ToString();
    }

    public static string GetAssignedKey(string inputName, bool preferController = false)
    {
        if (string.IsNullOrEmpty(inputName))
            return "—";

#if UNITY_EDITOR
        var obj = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
        var axes = obj.FindProperty("m_Axes");

        for (int i = 0; i < axes.arraySize; i++)
        {
            var axis = axes.GetArrayElementAtIndex(i);
            if (axis.FindPropertyRelative("m_Name").stringValue != inputName)
                continue;

            string negKey = axis.FindPropertyRelative("negativeButton").stringValue;
            string posKey = axis.FindPropertyRelative("positiveButton").stringValue;

            if (!string.IsNullOrEmpty(negKey) && !string.IsNullOrEmpty(posKey))
                return PlayerControls.FormatBindingKeyString(negKey + " / " + posKey);

            if (!string.IsNullOrEmpty(posKey))
                return PlayerControls.FormatBindingKeyString(posKey);

            string alt = axis.FindPropertyRelative("altPositiveButton").stringValue;
            if (!string.IsNullOrEmpty(alt))
                return PlayerControls.FormatBindingKeyString(alt);

            return inputName;
        }
#else
        if (PlayerControls.TryGetRuntimeDefaultKeyDisplay(inputName, out string runtime))
            return runtime;
#endif
        return inputName;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ControlLabel))]
public class ControlLabelEditor : Editor
{
    SerializedProperty _bindingFieldName;
    SerializedProperty _controlsOverride;

    SerializedProperty _textFormat;

    void OnEnable()
    {
        _bindingFieldName = serializedObject.FindProperty("bindingFieldName");
        _controlsOverride = serializedObject.FindProperty("controlsOverride");
        _textFormat = serializedObject.FindProperty("textFormat");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUILayout.PropertyField(_controlsOverride);
        EditorGUILayout.PropertyField(_textFormat);

        string[] fields = ControlLabel.GetPlayerControlsBindingFieldNames();
        if (fields.Length == 0)
        {
            EditorGUILayout.HelpBox("No instance string fields found on PlayerControls.", MessageType.Warning);
        }
        else
        {
            string current = _bindingFieldName.stringValue;
            int safeIdx = Array.IndexOf(fields, current);
            int displayIdx = safeIdx >= 0 ? safeIdx : 0;
            if (safeIdx < 0)
            {
                EditorGUILayout.HelpBox(
                    $"Field \"{current}\" is not on PlayerControls. Pick a binding below.",
                    MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            int newIdx = EditorGUILayout.Popup("Input binding", displayIdx, fields);
            if (EditorGUI.EndChangeCheck())
                _bindingFieldName.stringValue = fields[newIdx];
        }

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Refresh label text"))
        {
            foreach (UnityEngine.Object t in targets)
                ((ControlLabel)t).Refresh();
        }
    }
}
#endif
