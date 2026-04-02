using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Two looping layers (explore + chase) on the music mixer bus. Each frame, checks
/// <see cref="EnemyAI.AnyEnemyInChaseMusicState"/> — chase layer fades in if any enemy has
/// <see cref="EnemyAI.IsInChaseMusicState"/>; otherwise explore.
/// Optional one-shot <see cref="searchCue"/> when any enemy enters search (<see cref="NotifyEnemyEnteredSearch"/>).
/// When <see cref="PauseMenu.isPaused"/>, explore/chase volumes can be scaled via <see cref="duckLoopsWhilePaused"/>.
/// </summary>
[DefaultExecutionOrder(20)]
public class DynamicMusic : MonoBehaviour
{
    [SerializeField] AudioClip exploreMusic;
    [SerializeField] AudioClip chaseMusic;

    [Header("Search sting")]
    [Tooltip("One-shot through the music bus when an enemy starts searching (lost you / investigating).")]
    [SerializeField] AudioClip searchCue;
    [Tooltip("Minimum real-time seconds between search stings (uses unscaled time).")]
    [SerializeField] float searchCueCooldown = 2.5f;
    [SerializeField] [Range(0f, 1f)] float searchCueVolume = 1f;
    [Tooltip("Seconds to crossfade fully between layers.")]
    [SerializeField] float crossfadeDuration = 2f;
    [Tooltip("AudioSource volume multiplier before mixer (Music slider still applies via group).")]
    [SerializeField] [Range(0f, 1f)] float volume = 1f;
    [SerializeField] bool playOnEnable = true;
    [Tooltip("When true, crossfade uses unscaled delta time (keeps responding while gameplay is paused).")]
    [SerializeField] bool useUnscaledTime = true;

    [Header("Pause (reads PauseMenu.isPaused)")]
    [Tooltip("Lower explore/chase loop volume while the game is paused.")]
    [SerializeField] bool duckLoopsWhilePaused = true;
    [Tooltip("0 = mute loops, 1 = no change while paused. Typical: 0.25–0.5.")]
    [SerializeField] [Range(0f, 1f)] float pausedLoopVolumeMultiplier = 0.35f;

    [SerializeField] private AudioMixerGroup musicOutputOverride;

    AudioSource _explore;
    AudioSource _chase;
    AudioSource _stinger;
    float _chaseWeight;
    float _targetChaseWeight;

    static DynamicMusic s_searchCueHost;
    static float s_lastSearchCueUnscaledTime = -999f;
    static int s_lastSearchCueFrame = -1;

    /// <summary>Called from <see cref="EnemyAI"/> when entering search.</summary>
    public static void NotifyEnemyEnteredSearch()
    {
        var h = s_searchCueHost;
        if (h == null || h.searchCue == null || h._stinger == null) return;

        // Cooldown is always in real seconds (unscaled), independent of explore/chase crossfade option.
        float t = Time.unscaledTime;
        // Enter Play Mode without domain reload: static survives while unscaled time resets — unstick cooldown.
        if (s_lastSearchCueUnscaledTime > t + 0.02f)
            s_lastSearchCueUnscaledTime = -999f;

        float cd = Mathf.Max(0f, h.searchCueCooldown);
        if (cd > 0f)
        {
            if (t < s_lastSearchCueUnscaledTime + cd)
                return;
        }
        else if (Time.frameCount == s_lastSearchCueFrame)
        {
            // Cooldown 0: still at most one sting per frame (multiple enemies → EnterSearch same frame).
            return;
        }

        s_lastSearchCueUnscaledTime = t;
        s_lastSearchCueFrame = Time.frameCount;
        float v = Mathf.Clamp01(h.searchCueVolume) * Mathf.Clamp01(h.volume);
        h._stinger.PlayOneShot(h.searchCue, v);
    }

    void Awake()
    {
        _explore = gameObject.AddComponent<AudioSource>();
        _chase = gameObject.AddComponent<AudioSource>();
        _stinger = gameObject.AddComponent<AudioSource>();
        _stinger.loop = false;
        _stinger.playOnAwake = false;
        _stinger.spatialBlend = 0f;

        foreach (var s in new[] { _explore, _chase })
        {
            s.playOnAwake = false;
            s.loop = true;
            s.spatialBlend = 0f;
        }

        _explore.clip = exploreMusic;
        _chase.clip = chaseMusic;

        ApplyMixerRoute(_explore);
        ApplyMixerRoute(_chase);
        ApplyMixerRoute(_stinger);

        _targetChaseWeight = EnemyAI.AnyEnemyInChaseMusicState() ? 1f : 0f;
        _chaseWeight = _targetChaseWeight;
        ApplyVolumes();
    }

    void OnEnable()
    {
        if (searchCue != null)
            s_searchCueHost = this;

        if (!playOnEnable)
            return;

        if (exploreMusic != null && !_explore.isPlaying)
            _explore.Play();
        if (chaseMusic != null && !_chase.isPlaying)
            _chase.Play();
    }

    void OnDisable()
    {
        if (s_searchCueHost == this)
            s_searchCueHost = null;

        if (_explore != null) _explore.Stop();
        if (_chase != null) _chase.Stop();
    }

    void Update()
    {
        _targetChaseWeight = EnemyAI.AnyEnemyInChaseMusicState() ? 1f : 0f;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float speed = crossfadeDuration > 0.0001f ? 1f / crossfadeDuration : 100f;
        _chaseWeight = Mathf.MoveTowards(_chaseWeight, _targetChaseWeight, speed * dt);
        ApplyVolumes();
    }

    void ApplyVolumes()
    {
        float pauseMul = 1f;
        if (duckLoopsWhilePaused && PauseMenu.isPaused)
            pauseMul = Mathf.Clamp01(pausedLoopVolumeMultiplier);

        float v = Mathf.Clamp01(volume) * pauseMul;
        // Constant-power crossfade: linear (1-w)/w on two buses sums hotter than MenuMusic’s single source.
        float w = Mathf.Clamp01(_chaseWeight);
        float exploreScalar = Mathf.Cos(w * (Mathf.PI * 0.5f));
        float chaseScalar = Mathf.Sin(w * (Mathf.PI * 0.5f));
        if (_explore != null)
            _explore.volume = v * exploreScalar;
        if (_chase != null)
            _chase.volume = v * chaseScalar;
    }

    void ApplyMixerRoute(AudioSource s)
    {
        if (s == null) return;
        if (musicOutputOverride != null)
            s.outputAudioMixerGroup = musicOutputOverride;
        else if (GameAudio.MusicOutputGroup != null)
            s.outputAudioMixerGroup = GameAudio.MusicOutputGroup;
    }

    void Start()
    {
        ApplyMixerRoute(_explore);
        ApplyMixerRoute(_chase);
        if (GameAudio.MusicOutputGroup == null && musicOutputOverride == null)
        {
            foreach (var boot in FindObjectsByType<SettingsAudioBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                boot.ApplyNow();
                break;
            }
            ApplyMixerRoute(_explore);
            ApplyMixerRoute(_chase);
            ApplyMixerRoute(_stinger);
        }
    }
}
