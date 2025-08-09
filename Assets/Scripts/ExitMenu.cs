using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExitMenu : MonoBehaviour
{
    public Button quitButton;
    public Button mainMenuButton;
    public AudioClip clickSound;
    public AudioClip victorySound;
    public float clickDelay = 0.3f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (victorySound != null)
            audioSource.PlayOneShot(victorySound);

        if (quitButton != null)
            quitButton.onClick.AddListener(() => StartCoroutine(QuitGameWithSound()));

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => StartCoroutine(LoadMainMenuWithSound()));
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

    IEnumerator LoadMainMenuWithSound()
    {
        PlayClick();
        yield return new WaitForSeconds(clickDelay);
        Debug.Log("Loading Main Menu...");
        SceneManager.LoadScene("MainMenu");
    }

    void PlayClick()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
