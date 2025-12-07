using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
public class StartUiAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip rulesPanelMusic;
    public AudioClip settingsPanelMusic;

    [Header("SFX Clips")]
    public AudioClip hoverSfx;
    public AudioClip buttonClip;
    public AudioClip closeClip;
    public AudioClip playClip;
    public AudioClip quitClip;

    [Header("Volumes")]
    [Range(0f,1f)] public float musicVolume = 0.65f;
    [Range(0f,1f)] public float sfxVolume = 1f;

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
                Debug.LogWarning("UiAudio: No UIDocument found in scene");
                return;
            }
        }

        RegisterButtons(doc.rootVisualElement);

        if (mainMenuMusic != null)
            StartCoroutine(FadeToMusic(mainMenuMusic, fadeTime));
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
            if (registeredButtons.Contains(b)) continue;

            // hover -> play hoverSfx
            EventCallback<PointerEnterEvent> hoverCb = (evt) => { if (hoverSfx) PlaySfx(hoverSfx); };
            b.RegisterCallback(hoverCb);
            hoverMap[b] = hoverCb;

            // click -> play buttonClip (generic) and special clips for named buttons
            System.Action clickCb = () => PlaySfx(buttonClip);
            b.clicked += clickCb;
            clickMap[b] = clickCb;

            // special named behaviors
            switch (b.name)
            {
                case "close-rules":
                case "close-settings":
                    b.clicked += () => StartCoroutine(FadeToMusicCoroutine(mainMenuMusic));
                    b.clicked += () => PlaySfx(closeClip);
                    break;
                case "rules-button":
                    b.clicked += () => StartCoroutine(FadeToMusicCoroutine(rulesPanelMusic));
                    break;
                case "settings-button":
                    b.clicked += () => StartCoroutine(FadeToMusicCoroutine(settingsPanelMusic));
                    break;
                case "play-button":
                    b.clicked += () => PlaySfx(playClip);
                    break;
                case "quit-button":
                    b.clicked += () => PlaySfx(quitClip);
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

    public IEnumerator FadeToMusic(AudioClip newClip, float time)
    {
        if (musicSource == null) yield break;

        float start = musicSource.isPlaying ? musicSource.volume : 0f;
        // fade out
        for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(start, 0f, t / time);
            yield return null;
        }
        musicSource.volume = 0f;

        musicSource.clip = newClip;
        musicSource.Play();

        // fade in
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
}
