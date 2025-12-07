using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Timeline;


/// <summary>
/// In-game audio manager (similar in spirit to StartUiAudio but for gameplay events).
/// Handles: game music, volleyball hits, seagull hits, per-side scoring, and ball ground hits.
/// Attach this to a persistent GameObject in the Main scene and assign clips in the inspector.
/// Call the public methods from gameplay scripts (BallController, VolleyballGameManager, SeagullController).
/// </summary>
public class GameAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;

    [Header("Audio Mixer Groups")]
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup ambientMixerGroup;

    [Header("Music")]
    public AudioClip gameMusic;

    [Range(0f,1f)] public float musicVolume = 0.7f;
    public float musicFadeTime = 0.35f;

    [Header("SFX Clips")]
    public AudioClip BackgroundOcean;
    public AudioClip[] volleyballHitClips;
    public AudioClip[] seagullHitClips;
    public AudioClip scoreLeftClip;
    public AudioClip scoreRightClip;
    public AudioClip groundHitClip;
    public AudioClip victoryClip;
    public AudioClip scoreClip; 

    public AudioClip[] footstepClip;
    public AudioClip jumpClip;
    [Range(0f,1f)] public float ambientVolume = 0.9f;
    [Range(0f,1f)] public float sfxVolume = 1f;

    // Optional singleton for easy access
    public static GameAudio Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }

        // Assign mixer groups if provided
        if (musicMixerGroup != null)
            musicSource.outputAudioMixerGroup = musicMixerGroup;
        if (sfxMixerGroup != null)
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        if (ambientMixerGroup != null)
            ambientSource.outputAudioMixerGroup = ambientMixerGroup;
        else if (sfxMixerGroup != null)
            ambientSource.outputAudioMixerGroup = sfxMixerGroup;

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        ambientSource.volume = ambientVolume;
    }

    void OnEnable()
    {
        if (gameMusic != null)
            StartCoroutine(FadeToMusic(gameMusic, musicFadeTime));

        if (BackgroundOcean != null)
            PlayBackgroundOcean();
    }

    void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    //Public API for gameplay scripts
    public void PlayVolleyballHit() => PlaySfx(GetRandomClip(volleyballHitClips));
    public void PlaySeagullHit()    => PlaySfx(GetRandomClip(seagullHitClips));
    public void PlayFootstep()      => PlaySfx(GetRandomClip(footstepClip));
    public void PlayScoreLeft()     => PlaySfx(scoreLeftClip);
    public void PlayScoreRight()    => PlaySfx(scoreRightClip);
    public void PlayGroundHit()     => PlaySfx(groundHitClip);
    public void PlayVictory()       => PlaySfx(victoryClip);
    public void PlayScore()         => PlaySfx(scoreClip);
    public void PlayBackgroundOcean()
    {
        if (ambientSource == null || BackgroundOcean == null) return;
        if (ambientSource.clip == BackgroundOcean && ambientSource.isPlaying) return;
        ambientSource.clip = BackgroundOcean;
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        ambientSource.Play();
    }


    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public IEnumerator FadeToMusic(AudioClip newClip, float time)
    {
        if (musicSource == null || newClip == null) yield break;

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
}
