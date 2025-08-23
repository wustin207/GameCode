using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Music : MonoBehaviour
{
    #region Instance
    [Header("Instance")]
    public static Music Instance;
    #endregion

    #region Audio Settings
    [Header("Audio Settings")]
    public AudioSource musicSource;
    public AudioClip menuMusic;
    public AudioClip ingameMusic;
    public float fadeDuration = 1.5f;
    #endregion

    #region Sound Clips
    [Header("Sound Clips")]
    private AudioClip currentClip;
    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        //Play music based on current active scene
        UpdateMusic(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Change the music on scene change
        UpdateMusic(scene.name);
    }

    private void UpdateMusic(string sceneName)
    {
        if (sceneName == "SampleScene")
        {
            //Fade to the new music so it smoothly transitions
            if (currentClip != ingameMusic)
                StartCoroutine(FadeToNewMusic(ingameMusic));
        }
        else if (sceneName == "Lose" || sceneName == "Win" || sceneName == "MainMenu")
        {
            //Fade to the new music so it smoothly transitions
            if (currentClip != menuMusic)
                StartCoroutine(FadeToNewMusic(menuMusic));
        }
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        if (musicSource == null || newClip == null) yield break;

        float startVolume = musicSource.volume;

        //Fade out for a smooth transition
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();
        currentClip = newClip;

        //Fade in for a smooth transition
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = startVolume;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
