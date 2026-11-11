using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;

    [Header("Para no pisar otras pantallas")]
    public GameManager game;            // opcional
    public GameObject levelUpPanel;     // opcional

    public bool IsPaused { get; private set; }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // P o Escape (si solo querés la P, borrá la parte de escapeKey)
        bool pressed = Keyboard.current.pKey.wasPressedThisFrame
                    || Keyboard.current.escapeKey.wasPressedThisFrame;
        if (!pressed) return;

        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;
        if (game != null && game.State != GameState.Playing) return;                 // ya terminó la partida
        if (levelUpPanel != null && levelUpPanel.activeInHierarchy) return;          // eligiendo mejora

        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}