using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Plays the background music of each scene and all sound effects. Created automatically when the game
// starts and kept across scenes. Clips are set in Resources/AudioLibrary; volumes are saved in PlayerPrefs.
public class AudioManager : MonoBehaviour
{
    private const string BgmVolumeKey = "Settings.BgmVolume";
    private const string SfxVolumeKey = "Settings.SfxVolume";
    private const float MusicFadeDuration = 0.8f;

    private static AudioManager instance;

    private AudioLibrary library;
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private SoundClip currentMusic;
    private Coroutine musicFade;
    private readonly List<RaycastResult> raycastResults = new();

    public float BgmVolume { get; private set; }
    public float SfxVolume { get; private set; }

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject audioObject = new("AudioManager");
                DontDestroyOnLoad(audioObject);
                instance = audioObject.AddComponent<AudioManager>();
            }
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnStart()
    {
        _ = Instance;
    }

    public static void PlaySfx(Sfx sfx)
    {
        Instance.PlayOneShot(Instance.library.Get(sfx));
    }

    private void Awake()
    {
        library = Resources.Load<AudioLibrary>("AudioLibrary");
        if (library == null) library = ScriptableObject.CreateInstance<AudioLibrary>();

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, library.defaultBgmVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, library.defaultSfxVolume);

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
        if (musicFade == null) musicSource.volume = MusicVolume(currentMusic);
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
    }

    private void PlayMusicForScene(string sceneName)
    {
        SoundClip music = library.GetSceneMusic(sceneName);
        bool bossFight = sceneName == "Battle" && GameDataManager.Instance != null && GameDataManager.Instance.indexEnemy == 2;
        if (bossFight && library.bossBattleMusic.clip != null) music = library.bossBattleMusic;
        if (music == null || music.clip == null) return; // not listed: keep the current music

        if (currentMusic != null && currentMusic.clip == music.clip && musicSource.isPlaying) return;
        if (musicFade != null) StopCoroutine(musicFade);
        musicFade = StartCoroutine(SwitchMusic(music));
    }

    private IEnumerator SwitchMusic(SoundClip music)
    {
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            for (float t = 0f; t < MusicFadeDuration; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / MusicFadeDuration);
                yield return null;
            }
        }

        currentMusic = music;
        musicSource.clip = music.clip;
        musicSource.volume = 0f;
        musicSource.Play();
        for (float t = 0f; t < MusicFadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, MusicVolume(music), t / MusicFadeDuration);
            yield return null;
        }
        musicSource.volume = MusicVolume(music);
        musicFade = null;
    }

    private float MusicVolume(SoundClip music)
    {
        return music == null ? 0f : music.volume * BgmVolume;
    }

    private void PlayOneShot(SoundClip sound)
    {
        if (sound == null || sound.clip == null) return;
        sfxSource.PlayOneShot(sound.clip, sound.volume * SfxVolume);
    }

    // One click sound for every clickable UI element (buttons, menu texts, map nodes, piles...).
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || EventSystem.current == null) return;

        PointerEventData pointer = new(EventSystem.current) { position = Input.mousePosition };
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointer, raycastResults);
        if (raycastResults.Count == 0) return;

        GameObject clickable = ExecuteEvents.GetEventHandler<IPointerClickHandler>(raycastResults[0].gameObject);
        if (clickable == null) return;
        Selectable selectable = clickable.GetComponent<Selectable>();
        if (selectable != null && !selectable.IsInteractable()) return;

        PlayOneShot(library.buttonClick);
    }
}
