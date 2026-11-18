using System.Collections.Generic;
using UnityEngine;

public static class MetaProgress
{
    [System.Serializable]
    class SaveData
    {
        public int scrap;
        public List<string> ids = new List<string>();
        public List<int> levels = new List<int>();
    }

    const string Key = "tank_meta_progress";

    static SaveData data;
    static bool loaded;

    public static int RunScrap { get; private set; }     // chatarra de la partida actual
    public static int Scrap { get { Load(); return data.scrap; } }   // banco permanente

    static void Load()
    {
        if (loaded) return;
        loaded = true;

        string json = PlayerPrefs.GetString(Key, "");
        data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
        if (data == null) data = new SaveData();
    }

    static void Save()
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static int GetLevel(MetaUpgradeData u)
    {
        Load();
        int i = data.ids.IndexOf(u.id);
        return i >= 0 ? data.levels[i] : 0;
    }

    public static bool CanBuy(MetaUpgradeData u)
    {
        int level = GetLevel(u);
        return level < u.maxLevel && Scrap >= u.CostForLevel(level);
    }

    public static bool TryBuy(MetaUpgradeData u)
    {
        if (!CanBuy(u)) return false;

        int level = GetLevel(u);
        data.scrap -= u.CostForLevel(level);

        int i = data.ids.IndexOf(u.id);
        if (i >= 0) data.levels[i] = level + 1;
        else { data.ids.Add(u.id); data.levels.Add(1); }

        Save();
        return true;
    }
    static string UnlockId(UpgradeData u) => "unlock_" + u.name;

    public static bool IsUnlocked(UpgradeData u)
    {
        Load();
        return data.ids.Contains(UnlockId(u));
    }

    public static bool CanUnlock(UpgradeData u) => !IsUnlocked(u) && Scrap >= u.unlockCost;

    public static bool TryUnlock(UpgradeData u)
    {
        if (!CanUnlock(u)) return false;
        data.scrap -= u.unlockCost;
        data.ids.Add(UnlockId(u));
        data.levels.Add(1);
        Save();
        return true;
    }

    // ---- partida ----

    public static void StartRun() => RunScrap = 0;

    public static void AddScrap(int amount) => RunScrap += Mathf.Max(0, amount);

    // Pasa la chatarra de la partida al banco y devuelve cuánta se sumó
    public static int EndRun(float multiplier)
    {
        Load();
        int gained = Mathf.RoundToInt(RunScrap * multiplier);
        data.scrap += gained;
        Save();
        return gained;
    }

    // ---- solo para pruebas ----

    public static void DebugAddScrap(int amount)
    {
        Load();
        data.scrap += amount;
        Save();
    }

    public static void ResetAll()
    {
        data = new SaveData();
        loaded = true;
        Save();
    }
}