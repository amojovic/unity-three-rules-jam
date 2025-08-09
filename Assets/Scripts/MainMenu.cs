using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;
    public Toggle musicToggle;
    public AudioClip clickSound;
    public float clickDelay = 0.3f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        playButton.onClick.AddListener(() => StartCoroutine(PlayGameWithSound()));
        quitButton.onClick.AddListener(() => StartCoroutine(QuitGameWithSound()));

        if (PlayerPrefs.HasKey("MusicOn"))
            musicToggle.isOn = PlayerPrefs.GetInt("MusicOn") == 1;
        else
            musicToggle.isOn = true;

        musicToggle.onValueChanged.AddListener(delegate {
            if (MusicManager.Instance != null)
                MusicManager.Instance.ToggleMusic(musicToggle.isOn);
        });

        if (MusicManager.Instance != null && !MusicManager.Instance.GetComponent<AudioSource>().isPlaying)
        {
            MusicManager.Instance.PlayMusic(MusicManager.Instance.GetComponent<AudioSource>().clip);
            MusicManager.Instance.ToggleMusic(musicToggle.isOn);
        }
    }

    IEnumerator PlayGameWithSound()
    {
        PlayClick();
        yield return new WaitForSeconds(clickDelay);
        SceneManager.LoadScene("GameScene");
    }

    IEnumerator QuitGameWithSound()
    {
        PlayClick();
        yield return new WaitForSeconds(clickDelay);
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void PlayClick()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
