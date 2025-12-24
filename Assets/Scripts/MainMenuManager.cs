using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject loadingPanel;

    [Header("Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public Button backButton;

    [Header("Text Elements")]
    public TMP_Text titleText;
    public TMP_InputField playerNameInput;

    [Header("Audio")]
    public AudioSource backgroundMusic;

    string gameSceneName = "KORIG";

    [Header("Animation Settings")]
    public float titleGlowSpeed = 2f;

    private bool isOnMainMenu = true;
    private Color titleOriginalColor;

    private void Start()
    {
        loadingPanel.SetActive(false);
        ShowMainMenu();

        playerNameInput.text = PlayerPrefs.GetString("PlayerName", "");

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(ExitGame);

        if (backButton != null)
            backButton.onClick.AddListener(ShowMainMenu);

        if (titleText != null)
        {
            titleOriginalColor = titleText.color;
        }

        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }
    }

    private void Update()
    {
        if (titleText != null && isOnMainMenu)
        {
            float glow = Mathf.PingPong(Time.time * titleGlowSpeed, 1f);
            titleText.color = Color.Lerp(titleOriginalColor, Color.white, glow * 0.3f);
        }
    }

    public void StartGame()
    {
        Debug.Log("Oyun baslatiliyor...");
        PlayerPrefs.SetString("PlayerName", playerNameInput.text);
        loadingPanel.SetActive(true);
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowSettings()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        isOnMainMenu = false;
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        isOnMainMenu = true;
    }

    public void ExitGame()
    {
        Debug.Log("Oyundan cikiliyor...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}