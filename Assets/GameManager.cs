using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Playing, Won, Lost }

public class GameManager : MonoBehaviour
{
    [Header("Datos")]
    public MatchConfig config;

    [Header("Referencias")]
    public Health playerHealth;
    public PlayerXP playerXP;
    public WaveSpawner spawner;

    [Header("Pantallas")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public TMP_Text victoryStatsText;
    public TMP_Text defeatStatsText;

    public GameState State { get; private set; } = GameState.Playing;
    public float ElapsedTime { get; private set; }
    public float TimeLeft => config != null ? Mathf.Max(0f, config.survivalTime - ElapsedTime) : 0f;
    public event Action<GameState> OnStateChanged;

    void Start()
    {
        Time.timeScale = 1f;

        if (config == null)
        {
            Debug.LogError("GameManager: falta asignar el MatchConfig", this);
            enabled = false;
            return;
        }

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (playerHealth == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerHealth = p.GetComponent<Health>();
        }
        if (playerHealth != null) playerHealth.OnDied += OnPlayerDied;
        int enemy = LayerMask.NameToLayer("Enemy");
        int player = LayerMask.NameToLayer("Player");
        if (enemy >= 0 && player >= 0)
        {
            Physics.IgnoreLayerCollision(enemy, enemy, true);     // enemigos no se empujan entre sí
            Physics.IgnoreLayerCollision(enemy, player, true);    // ni empujan al jugador
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnDied -= OnPlayerDied;
    }

    void Update()
    {
        if (State != GameState.Playing) return;

        ElapsedTime += Time.deltaTime;     // se frena solo cuando el juego está en pausa
        if (ElapsedTime >= config.survivalTime) End(GameState.Won);
    }

    void OnPlayerDied()
    {
        if (State == GameState.Playing) End(GameState.Lost);
    }

    void End(GameState result)
    {
        State = result;
        Time.timeScale = 0f;

        if (result == GameState.Won)
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (victoryStatsText != null) victoryStatsText.text = BuildStats();
        }
        else
        {
            if (defeatPanel != null) defeatPanel.SetActive(true);
            if (defeatStatsText != null) defeatStatsText.text = BuildStats();
        }

        OnStateChanged?.Invoke(result);
    }

    string BuildStats()
    {
        int t = Mathf.FloorToInt(ElapsedTime);
        string s = $"Tiempo: {t / 60:00}:{t % 60:00}";
        if (playerXP != null) s += $"\nNivel: {playerXP.level}";
        if (spawner != null) s += $"\nOleada: {spawner.Wave}";
        return s;
    }

    // Para los botones de las pantallas
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}