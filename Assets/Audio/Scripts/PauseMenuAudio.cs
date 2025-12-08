using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Handles audio ONLY for the in-game pause menu using UI Toolkit.
/// Plays SFX on button interactions, fades to pause menu music when menu opens.
/// When resumed, fades out and stops playing, returning control to the default audio manager.
/// Mirrors the StartUiAudio pattern.
/// Attached to the same GameObject as PauseMenuController.
/// </summary>
public class PauseMenuAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip pauseMenuMusic;
    public AudioClip rulesMusic;
    public AudioClip settingsMusic;

    [Header("SFX Clips")]
    public AudioClip hoverSfx;
    public AudioClip buttonClickSfx;
    public AudioClip panelOpenSfx;
    public AudioClip panelCloseSfx;
    public AudioClip resumeSfx;

    [Header("Volumes")]
    [Range(0f, 1f)] public float musicVolume = 0.65f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Music Fade")]
    public float fadeTime = 0.15f;

    // Runtime lists so we can unregister callbacks
    private UIDocument doc;
    private List<Button> registeredButtons = new List<Button>();
    private Dictionary<Button, System.Action> clickMap = new Dictionary<Button, System.Action>();
    private Dictionary<Button, EventCallback<PointerEnterEvent>> hoverMap = new Dictionary<Button, EventCallback<PointerEnterEvent>>();

    void Awake()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            doc = FindObjectOfType<UIDocument>();
            if (doc == null)
            {
                Debug.LogWarning("PauseMenuAudio: No UIDocument found in scene");
                return;
            }
        }

        RegisterButtons(doc.rootVisualElement);
    }

    void OnDisable()
    {
        UnregisterButtons();
    }

    private void RegisterButtons(VisualElement root)
    {
        var buttons = root.Query<Button>().ToList();
        foreach (var b in buttons)
        {
            // Skip if already registered
            if (registeredButtons.Contains(b)) continue;

            // hover -> play hoverSfx
            EventCallback<PointerEnterEvent> hoverCb = (evt) => { if (hoverSfx) PlaySfx(hoverSfx); };
            b.RegisterCallback(hoverCb);
            hoverMap[b] = hoverCb;

            // click -> play buttonClickSfx (generic) and special clips for named buttons
            System.Action clickCb = () => PlaySfx(buttonClickSfx);
            b.clicked += clickCb;
            clickMap[b] = clickCb;

            // special named behaviors for pause menu buttons
            switch (b.name)
            {
                case "resume-button":
                    b.clicked += () => PlaySfx(resumeSfx);
                    break;
                case "pause-settings-button":
                    b.clicked += () => StartCoroutine(FadeToMusicCoroutine(settingsMusic));
                    b.clicked += () => PlaySfx(panelOpenSfx);
                    break;
                case "pause-rules-button":
                    b.clicked += () => StartCoroutine(FadeToMusicCoroutine(rulesMusic));
                    b.clicked += () => PlaySfx(panelOpenSfx);
                    break;
                case "pause-back-from-settings":
                case "pause-back-from-rules":
                    b.clicked += () => StartCoroutine(FadeToMusicCoroutine(pauseMenuMusic));
                    b.clicked += () => PlaySfx(panelCloseSfx);
                    break;
                case "pause-quit-button":
                    // Quit doesn't need special audio
                    break;
            }

            registeredButtons.Add(b);
        }
    }

    private void UnregisterButtons()
    {
        foreach (var b in registeredButtons)
        {
            if (hoverMap.TryGetValue(b, out var hb))
                b.UnregisterCallback(hb);
            if (clickMap.TryGetValue(b, out var cb))
                b.clicked -= cb;
        }

        registeredButtons.Clear();
        hoverMap.Clear();
        clickMap.Clear();
    }


    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// Call this when the pause menu is shown to fade to pause menu music.
    /// </summary>
    public void OnPauseMenuShown()
    {
        if (pauseMenuMusic != null)
            StartCoroutine(FadeToMusic(pauseMenuMusic, fadeTime));
    }

    /// <summary>
    /// Call this when the game resumes to fade out pause menu music and stop playing.
    /// Control returns to the default audio manager in the scene.
    /// </summary>
    public void OnGameResumed()
    {
        StartCoroutine(FadeOutMusic());
    }

    public IEnumerator FadeToMusic(AudioClip newClip, float time)
    {
        if (musicSource == null) yield break;

        float start = musicSource.isPlaying ? musicSource.volume : 0f;
        
        // Fade out
        for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(start, 0f, t / time);
            yield return null;
        }
        musicSource.volume = 0f;

        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t / time);
            yield return null;
        }
        musicSource.volume = musicVolume;
    }

    private IEnumerator FadeToMusicCoroutine(AudioClip clip)
    {
        if (clip == null) yield break;
        yield return StartCoroutine(FadeToMusic(clip, fadeTime));
    }

    private IEnumerator FadeOutMusic()
    {
        if (musicSource == null) yield break;

        // Fade out
        for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(musicVolume, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.volume = 0f;

        // Stop playing and clear clip
        musicSource.Stop();
        musicSource.clip = null;
    }
}

