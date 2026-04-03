using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Stable id for <see cref="SaveManager"/> so spawned duplicates (same hierarchy names) and runtime instances
/// save/load position and enemy state correctly. Assign <see cref="respawnPrefabTemplate"/> to this prefab asset
/// (drag the prefab root onto itself). Optional <see cref="resourcesSpawnPath"/> for player builds (Resources.Load).
/// </summary>
[DisallowMultipleComponent]
public class ScenePersistedIdentity : MonoBehaviour
{
    [SerializeField] string persistentId;

    [Tooltip("Drag this prefab’s root here (same prefab) so saves know what to instantiate on Continue.")]
    [SerializeField] GameObject respawnPrefabTemplate;

    [Tooltip("If set, used in player builds: path under a Resources folder, e.g. \"Stealth/Enemy\" for Resources/Stealth/Enemy.prefab")]
    [SerializeField] string resourcesSpawnPath;

    [SerializeField] string cachedRespawnPrefabGuid;

    public string PersistentId => persistentId;
    public string ResourcesSpawnPath => resourcesSpawnPath;
    public string CachedRespawnPrefabGuid => cachedRespawnPrefabGuid;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (respawnPrefabTemplate != null)
            cachedRespawnPrefabGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(respawnPrefabTemplate));
        else
            cachedRespawnPrefabGuid = null;
    }
#endif

    /// <summary>Called from <see cref="SaveManager.SaveLayout"/> before collecting so every tracked object has an id.</summary>
    public void EnsureGeneratedId()
    {
        if (string.IsNullOrEmpty(persistentId))
            persistentId = System.Guid.NewGuid().ToString("N");
    }

    /// <summary>After <see cref="Object.Instantiate"/> when restoring a saved instance.</summary>
    public void SetRestoredPersistedId(string id)
    {
        persistentId = id;
    }
}
