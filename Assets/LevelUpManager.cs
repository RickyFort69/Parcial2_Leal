using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelUpManager : MonoBehaviour
{
    [Header("Datos")]
    public UpgradePool pool;

    [Header("Referencias de la escena")]
    public PlayerXP playerXP;
    public PlayerStats playerStats;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text levelText;
    public Transform cardsContainer;
    public UpgradeCard cardPrefab;

    readonly Queue<int> pendingLevels = new Queue<int>();
    bool open;

    void OnEnable()
    {
        if (playerXP != null) playerXP.OnLevelUp += HandleLevelUp;
    }

    void OnDisable()
    {
        if (playerXP != null) playerXP.OnLevelUp -= HandleLevelUp;
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    void HandleLevelUp(int level)
    {
        pendingLevels.Enqueue(level);
        if (!open) ShowNext();
    }

    void ShowNext()
    {
        if (pendingLevels.Count == 0) { Close(); return; }

        int level = pendingLevels.Dequeue();
        var options = pool.GetRandom(pool.choicesPerLevel);
        if (options.Count == 0)
        {
            Debug.LogWarning("LevelUpManager: el UpgradePool no tiene mejoras", this);
            pendingLevels.Clear();
            Close();
            return;
        }

        open = true;
        Time.timeScale = 0f;
        panel.SetActive(true);
        if (levelText != null) levelText.text = $"¡NIVEL {level}!";

        // borrar las cartas de la tanda anterior
        foreach (Transform child in cardsContainer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // crear una carta por mejora
        foreach (var u in options)
        {
            var card = Instantiate(cardPrefab, cardsContainer);
            card.Setup(u, BuildStatLine(u), Choose);
        }
    }

    void Choose(UpgradeData u)
    {
        playerStats.Apply(u);
        ShowNext();   // si quedan niveles pendientes muestra otra tanda, si no cierra
    }

    void Close()
    {
        open = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    string BuildStatLine(UpgradeData u)
    {
        float now = playerStats.GetStat(u.stat);
        float next = playerStats.GetPreview(u);
        return $"{StatName(u.stat)}: {now:0.##} -> {next:0.##}";
    }

    static string StatName(StatType s) => s switch
    {
        StatType.Damage => "Daño",
        StatType.FireRate => "Cadencia",
        StatType.MoveSpeed => "Velocidad",
        StatType.TurnSpeed => "Giro",
        StatType.MaxHealth => "Vida máx.",
        StatType.PickupRange => "Rango XP",
        StatType.ProjectileSpeed => "Vel. de bala",
        StatType.Range => "Alcance",
        _ => s.ToString()
    };

    [ContextMenu("Probar level up")]
    void Test() => HandleLevelUp(playerXP.level + 1);
}