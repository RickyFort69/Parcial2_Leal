using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Datos")]
    public MetaUpgradePool pool;

    [Header("Textos y escenas (las dos escenas tienen que estar en Build Profiles)")]
    public string title = "TANKYRE SURVIVORS";
    public string gameScene = "Tankyre Survivors";

    [Header("Pruebas")]
    public bool devButtons = true;

    class ShopRow
    {
        public MetaUpgradeData data;
        public TMP_Text info, level, buyLabel;
        public Button buy;
    }

    static readonly Color BackColor = new Color(0.06f, 0.09f, 0.07f, 1f);
    static readonly Color PanelColor = new Color(0.12f, 0.17f, 0.12f, 1f);
    static readonly Color ButtonColor = new Color(0.30f, 0.40f, 0.22f, 1f);
    static readonly Color AccentColor = new Color(1f, 0.85f, 0.3f, 1f);

    RectTransform root;
    GameObject mainPanel, shopPanel;
    TMP_Text scrapText;
    readonly List<ShopRow> rows = new List<ShopRow>();

    void Awake()
    {
        Time.timeScale = 1f;
        EnsureEventSystem();
        BuildCanvas();
        BuildMainPanel();
        BuildShopPanel();
        BuildScrapText();
        ShowMain();
        Refresh();
    }

    // ---------- armado ----------

    void EnsureEventSystem()
    {
        var es = FindFirstObjectByType<EventSystem>();
        if (es == null)
            es = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();

        // el módulo viejo no funciona con el Input System nuevo
        var oldModule = es.GetComponent<StandaloneInputModule>();
        if (oldModule != null) Destroy(oldModule);

        if (es.GetComponent<InputSystemUIInputModule>() == null)
        {
            var module = es.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }

    void BuildCanvas()
    {
        var go = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        root = (RectTransform)go.transform;
        root.SetParent(transform, false);

        var bg = MakePanel("Fondo", root, BackColor);
        bg.GetComponent<Image>().raycastTarget = false;
    }

    void BuildMainPanel()
    {
        mainPanel = MakePanel("Principal", root, Color.clear);

        var t = MakeText("Titulo", mainPanel.transform, title, 130, TextAlignmentOptions.Center, AccentColor);
        t.fontStyle = FontStyles.Bold;
        Place(t.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1600f, 180f));

        MakeButton(mainPanel.transform, "Jugar", new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(520f, 100f), Play, out _);
        MakeButton(mainPanel.transform, "Mejoras", new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(520f, 100f), ShowShop, out _);
        MakeButton(mainPanel.transform, "Salir", new Vector2(0.5f, 0.5f), new Vector2(0f, -240f), new Vector2(520f, 100f), Quit, out _);

        if (Application.platform != RuntimePlatform.WebGLPlayer)
            MakeButton(mainPanel.transform, "Salir", new Vector2(0.5f, 0.5f), new Vector2(0f, -240f), new Vector2(520f, 100f), Quit, out _);
    }

    void BuildShopPanel()
    {
        shopPanel = MakePanel("Tienda", root, Color.clear);

        var header = MakeText("Titulo", shopPanel.transform, "MEJORAS PERMANENTES", 80, TextAlignmentOptions.Center, AccentColor);
        header.fontStyle = FontStyles.Bold;
        Place(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1400f, 110f));

        if (pool == null)
        {
            var warn = MakeText("Aviso", shopPanel.transform, "Falta asignar el MetaUpgradePool en el objeto MainMenu",
                                40, TextAlignmentOptions.Center, Color.white);
            Place(warn.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 100f));
        }
        else
        {
            foreach (var u in pool.upgrades)
                if (u != null) BuildRow(u, rows.Count);
        }

        MakeButton(shopPanel.transform, "Volver", new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(400f, 90f), ShowMain, out _);

        if (devButtons)
        {
            MakeButton(shopPanel.transform, "+100 chatarra (prueba)", new Vector2(0f, 0f), new Vector2(230f, 70f), new Vector2(420f, 80f),
                       () => { MetaProgress.DebugAddScrap(100); Refresh(); }, out _);
            MakeButton(shopPanel.transform, "Borrar progreso", new Vector2(0f, 0f), new Vector2(230f, 165f), new Vector2(420f, 80f),
                       () => { MetaProgress.ResetAll(); Refresh(); }, out _);
        }
    }

    void BuildRow(MetaUpgradeData u, int index)
    {
        float y = 170f + index * 105f;

        var rowGo = new GameObject("Fila" + index, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)rowGo.transform;
        rt.SetParent(shopPanel.transform, false);
        Place(rt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(1500f, 95f));
        var img = rowGo.GetComponent<Image>();
        img.color = PanelColor;
        img.raycastTarget = false;

        var row = new ShopRow { data = u };

        row.info = MakeText("Info", rt, "", 36, TextAlignmentOptions.MidlineLeft, Color.white);
        Place(row.info.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(25f, 0f), new Vector2(900f, 90f));

        row.level = MakeText("Nivel", rt, "", 40, TextAlignmentOptions.Midline, AccentColor);
        Place(row.level.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(960f, 0f), new Vector2(160f, 90f));

        row.buy = MakeButton(rt, "", new Vector2(1f, 0.5f), new Vector2(-160f, 0f), new Vector2(280f, 70f),
                             () => Buy(row), out row.buyLabel);
        rows.Add(row);
    }

    void BuildScrapText()
    {
        scrapText = MakeText("Chatarra", root, "", 48, TextAlignmentOptions.MidlineRight, AccentColor);
        Place(scrapText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(700f, 70f));
    }

    // ---------- acciones ----------

    void ShowMain() { mainPanel.SetActive(true); shopPanel.SetActive(false); }
    void ShowShop() { mainPanel.SetActive(false); shopPanel.SetActive(true); Refresh(); }

    void Play() => SceneManager.LoadScene(gameScene);

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Buy(ShopRow row)
    {
        if (MetaProgress.TryBuy(row.data)) Refresh();
    }

    void Refresh()
    {
        scrapText.text = "Chatarra: " + MetaProgress.Scrap;

        foreach (var r in rows)
        {
            var u = r.data;
            int level = MetaProgress.GetLevel(u);
            bool max = level >= u.maxLevel;

            string desc = string.IsNullOrEmpty(u.description)
                ? "+" + u.amountPerLevel + (u.isPercent ? "%" : "") + " " + StatName(u.stat) + " por nivel"
                : u.description;

            r.info.text = "<b>" + u.title + "</b>\n<size=70%>" + desc + "</size>";
            r.level.text = "Nv " + level + "/" + u.maxLevel;

            if (max)
            {
                r.buyLabel.text = "MAX";
                r.buy.interactable = false;
            }
            else
            {
                r.buyLabel.text = "Comprar (" + u.CostForLevel(level) + ")";
                r.buy.interactable = MetaProgress.CanBuy(u);
            }
        }
    }

    static string StatName(StatType s)
    {
        switch (s)
        {
            case StatType.Damage: return "daño";
            case StatType.FireRate: return "cadencia";
            case StatType.MoveSpeed: return "velocidad";
            case StatType.TurnSpeed: return "giro";
            case StatType.MaxHealth: return "vida máxima";
            case StatType.PickupRange: return "rango de recolección";
            case StatType.ProjectileSpeed: return "velocidad de bala";
            case StatType.Range: return "alcance";
            case StatType.HealthRegen: return "regeneración";
            default: return s.ToString();
        }
    }

    // ---------- utilidades de interfaz ----------

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    GameObject MakePanel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Stretch((RectTransform)go.transform);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return go;
    }

    TMP_Text MakeText(string name, Transform parent, string content, int size, TextAlignmentOptions align, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        t.overflowMode = TextOverflowModes.Overflow;
        return t;
    }

    Button MakeButton(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size,
                      UnityAction onClick, out TMP_Text labelText)
    {
        var go = new GameObject("Boton " + label, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Place(rt, anchor, new Vector2(0.5f, 0.5f), pos, size);

        var img = go.GetComponent<Image>();
        img.color = ButtonColor;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        labelText = MakeText("Texto", rt, label, 40, TextAlignmentOptions.Center, Color.white);
        Stretch(labelText.rectTransform);
        return btn;
    }
}