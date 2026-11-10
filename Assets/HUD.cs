using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Referencias")]
    public Health playerHealth;
    public PlayerXP playerXP;
    public WaveSpawner spawner;
    public GameManager game;

    [Header("Vida")]
    public Slider healthBar;
    public TMP_Text healthText;

    [Header("Experiencia")]
    public Slider xpBar;
    public TMP_Text levelText;

    [Header("Info")]
    public TMP_Text timerText;
    public TMP_Text waveText;

    void Update()
    {
        if (playerHealth != null)
        {
            float max = Mathf.Max(1f, playerHealth.maxHealth);
            if (healthBar != null) healthBar.value = playerHealth.current / max;
            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(playerHealth.current)} / {Mathf.CeilToInt(playerHealth.maxHealth)}";
        }

        if (playerXP != null)
        {
            if (xpBar != null) xpBar.value = (float)playerXP.currentXP / Mathf.Max(1, playerXP.xpToNext);
            if (levelText != null) levelText.text = $"Nivel {playerXP.level}";
        }

        if (game != null && timerText != null)
        {
            int s = Mathf.CeilToInt(game.TimeLeft);
            timerText.text = $"{s / 60:00}:{s % 60:00}";
        }

        if (spawner != null && waveText != null)
            waveText.text = $"Oleada {spawner.Wave}";
    }
}