using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FFBattleUI : MonoBehaviour
{
    [Header("Referencias")]
    public BattleManager battle;
    public FFUITheme theme;          // opcional: vacío = colores por defecto
    public Camera worldCamera;       // opcional: vacío = Main Camera

    // ---------- tipos internos ----------

    enum Mode { None, Command, Magic, Target }
    enum CommandId { Attack, Magic, Defend, Limit, Spell }

    class MenuEntry
    {
        public string label;
        public string right;
        public bool enabled = true;
        public CommandId id;
        public ActionData action;
    }

    class MenuList
    {
        public RectTransform window;
        public TMP_Text title;
        public TMP_Text[] left;
        public TMP_Text[] right;
        public List<MenuEntry> entries = new List<MenuEntry>();
        public int selected;
        public int scroll;
        public int Rows => left.Length;
    }

    class PartyRow
    {
        public Combatant c;
        public TMP_Text nameText, hpText, mpText;
        public RectTransform atbFill, limitFill;
        public Image atbImage;
        public int lastHp = -1, lastMaxHp = -1, lastMp = -1;
    }

    // ---------- estado ----------

    Camera Cam => worldCamera != null ? worldCamera : Camera.main;

    RectTransform root;
    RectTransform enemyWindow;
    readonly List<TMP_Text> enemyLabels = new List<TMP_Text>();
    RectTransform partyWindow;
    readonly List<PartyRow> rows = new List<PartyRow>();
    bool rowsBuilt;

    MenuList commandList, magicList;
    RectTransform targetWindow;
    TMP_Text targetText;
    RectTransform messageWindow;
    TMP_Text messageText;
    float messageTimer;
    TMP_Text waveText;
    int lastWave = -1;
    TMP_Text bigText;
    readonly List<Image> cursors = new List<Image>();

    Mode mode = Mode.None;
    Mode targetReturn;
    Combatant actor;
    ActionData pendingAction;
    List<Combatant> targetPool = new List<Combatant>();
    int targetIndex;
    bool targetAll;

    // ---------- ciclo de vida ----------

    void Awake()
    {
        if (battle == null) battle = FindFirstObjectByType<BattleManager>();
        if (theme == null) theme = ScriptableObject.CreateInstance<FFUITheme>();

        BuildCanvas();
        BuildEnemyWindow();
        commandList = BuildMenuList("Comandos", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-1070f, 30f), 320f, 4);
        magicList = BuildMenuList("Magia", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 320f), 640f, 5);
        BuildTargetWindow();
        BuildMessageWindow();
        BuildWaveWindow();
        BuildBigText();
        BuildCursors();

        commandList.window.gameObject.SetActive(false);
        magicList.window.gameObject.SetActive(false);
        targetWindow.gameObject.SetActive(false);
        messageWindow.gameObject.SetActive(false);
        bigText.gameObject.SetActive(false);
    }

    // Nos suscribimos en OnEnable (antes de que arranque el BattleManager) para no perder el primer aviso
    void OnEnable()
    {
        if (battle == null) return;
        battle.OnMessage += ShowMessage;
        battle.OnNumber += ShowNumber;
        battle.OnWaveStarted += OnWaveStarted;
        battle.OnStateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        if (battle == null) return;
        battle.OnMessage -= ShowMessage;
        battle.OnNumber -= ShowNumber;
        battle.OnWaveStarted -= OnWaveStarted;
        battle.OnStateChanged -= OnStateChanged;
    }

    void Update()
    {
        if (battle == null) return;

        EnsureRows();
        UpdateRows();
        UpdateEnemyLabels();
        UpdateMessage();
        UpdateWave();

        ManageMenu();
        HandleInput();
        UpdateCursors();

        battle.MenuOpen = mode != Mode.None;
    }

    // ---------- construcción de la interfaz ----------

    void BuildCanvas()
    {
        var go = new GameObject("FFBattleCanvas", typeof(Canvas), typeof(CanvasScaler));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        root = (RectTransform)go.transform;
        root.SetParent(transform, false);
    }

    void BuildEnemyWindow()
    {
        enemyWindow = MakeWindow("Enemigos", root, new Vector2(0f, 0f), new Vector2(0f, 0f),
                                 new Vector2(30f, 30f), new Vector2(480f, 240f));
    }

    MenuList BuildMenuList(string title, Vector2 anchor, Vector2 pivot, Vector2 pos, float width, int rowCount)
    {
        var l = new MenuList();
        float height = 44f + rowCount * 44f + 20f;
        l.window = MakeWindow(title, root, anchor, pivot, pos, new Vector2(width, height));

        l.title = MakeText("Title", l.window, title, theme.fontSize, TextAlignmentOptions.MidlineLeft, theme.textColor);
        PlaceTopStretch(l.title.rectTransform, 16f, 8f, 44f);

        l.left = new TMP_Text[rowCount];
        l.right = new TMP_Text[rowCount];
        for (int i = 0; i < rowCount; i++)
        {
            float y = 8f + 44f + i * 44f;
            l.left[i] = MakeText("Row" + i, l.window, "", theme.fontSize, TextAlignmentOptions.MidlineLeft, theme.textColor);
            l.right[i] = MakeText("RowRight" + i, l.window, "", theme.fontSize, TextAlignmentOptions.MidlineRight, theme.textColor);
            PlaceTopStretch(l.left[i].rectTransform, 16f, y, 44f);
            PlaceTopStretch(l.right[i].rectTransform, 16f, y, 44f);
        }
        return l;
    }

    void BuildTargetWindow()
    {
        targetWindow = MakeWindow("Objetivo", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                                  new Vector2(0f, -120f), new Vector2(700f, 70f));
        targetText = MakeText("Text", targetWindow, "", theme.fontSize, TextAlignmentOptions.Midline, theme.textColor);
        Stretch(targetText.rectTransform);
    }

    void BuildMessageWindow()
    {
        messageWindow = MakeWindow("Mensaje", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                                   new Vector2(0f, -30f), new Vector2(900f, 70f));
        messageText = MakeText("Text", messageWindow, "", theme.fontSize, TextAlignmentOptions.Midline, theme.textColor);
        Stretch(messageText.rectTransform);
    }

    void BuildWaveWindow()
    {
        var w = MakeWindow("Oleada", root, new Vector2(0f, 1f), new Vector2(0f, 1f),
                           new Vector2(30f, -30f), new Vector2(320f, 70f));
        waveText = MakeText("Text", w, "Oleada 1", theme.fontSize, TextAlignmentOptions.Midline, theme.textColor);
        Stretch(waveText.rectTransform);
    }

    void BuildBigText()
    {
        bigText = MakeText("Grande", root, "", 140, TextAlignmentOptions.Midline, theme.selectedColor);
        bigText.fontStyle = FontStyles.Bold;
        Place(bigText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 250f));
    }

    void BuildCursors()
    {
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject("Cursor" + i, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(root, false);
            rt.sizeDelta = new Vector2(32f, 32f);
            rt.localRotation = Quaternion.Euler(0f, 0f, 45f);

            var img = go.GetComponent<Image>();
            img.color = theme.selectedColor;
            img.raycastTarget = false;

            go.SetActive(false);
            cursors.Add(img);
        }
    }

    void EnsureRows()
    {
        // el grupo lo crea el BattleManager, así que esperamos a que exista
        if (rowsBuilt || battle.party.Count == 0) return;
        rowsBuilt = true;

        float height = 24f + battle.party.Count * 60f;
        partyWindow = MakeWindow("Grupo", root, new Vector2(1f, 0f), new Vector2(1f, 0f),
                                 new Vector2(-30f, 30f), new Vector2(1000f, height));

        for (int i = 0; i < battle.party.Count; i++)
        {
            var r = new PartyRow { c = battle.party[i] };
            float y = 12f + i * 60f;

            r.nameText = MakeText("Nombre", partyWindow, r.c.displayName, theme.fontSize, TextAlignmentOptions.MidlineLeft, theme.textColor);
            PlaceTopLeft(r.nameText.rectTransform, 24f, y, 240f, 56f);

            r.hpText = MakeText("HP", partyWindow, "", theme.fontSize, TextAlignmentOptions.MidlineRight, theme.textColor);
            PlaceTopLeft(r.hpText.rectTransform, 280f, y, 260f, 56f);

            r.mpText = MakeText("MP", partyWindow, "", theme.fontSize, TextAlignmentOptions.MidlineRight, theme.textColor);
            PlaceTopLeft(r.mpText.rectTransform, 550f, y, 170f, 56f);

            MakeBar(partyWindow, 740f, y + 10f, 240f, 20f, out r.atbFill, out r.atbImage);
            MakeBar(partyWindow, 740f, y + 36f, 240f, 10f, out r.limitFill, out Image limitImage);
            limitImage.color = theme.limitColor;

            rows.Add(r);
        }
    }

    void RebuildEnemyLabels()
    {
        foreach (var t in enemyLabels) if (t != null) Destroy(t.gameObject);
        enemyLabels.Clear();

        for (int i = 0; i < battle.enemies.Count; i++)
        {
            var t = MakeText("Enemigo" + i, enemyWindow, battle.enemies[i].displayName,
                             theme.fontSize, TextAlignmentOptions.MidlineLeft, theme.textColor);
            PlaceTopStretch(t.rectTransform, 24f, 14f + i * 52f, 48f);
            enemyLabels.Add(t);
        }
    }

    // ---------- actualización de pantalla ----------

    void UpdateRows()
    {
        foreach (var r in rows)
        {
            var c = r.c;

            if (c.hp != r.lastHp || c.maxHP != r.lastMaxHp)
            {
                r.hpText.text = "HP " + c.hp + "/" + c.maxHP;
                r.lastHp = c.hp;
                r.lastMaxHp = c.maxHP;
            }
            if (c.mp != r.lastMp)
            {
                r.mpText.text = "MP " + c.mp;
                r.lastMp = c.mp;
            }

            float ratio = c.maxHP > 0 ? (float)c.hp / c.maxHP : 0f;
            r.hpText.color = c.IsDead ? theme.disabledColor
                           : ratio <= 0.25f ? theme.hpCriticalColor
                           : ratio <= 0.5f ? theme.hpLowColor
                           : theme.textColor;
            r.nameText.color = c.IsDead ? theme.disabledColor
                             : c == actor ? theme.selectedColor
                             : theme.textColor;

            SetFill(r.atbFill, c.atb);
            r.atbImage.color = c.atb >= 1f ? theme.atbReadyColor : theme.atbColor;
            SetFill(r.limitFill, c.limit);
        }
    }

    void UpdateEnemyLabels()
    {
        for (int i = 0; i < enemyLabels.Count && i < battle.enemies.Count; i++)
            enemyLabels[i].gameObject.SetActive(!battle.enemies[i].IsDead);
    }

    void UpdateMessage()
    {
        if (!messageWindow.gameObject.activeSelf) return;
        messageTimer -= Time.deltaTime;
        if (messageTimer <= 0f) messageWindow.gameObject.SetActive(false);
    }

    void UpdateWave()
    {
        if (battle.CurrentWave == lastWave) return;
        lastWave = battle.CurrentWave;
        waveText.text = "Oleada " + lastWave;
    }

    void UpdateCursors()
    {
        int shown = 0;

        if (mode == Mode.Target && targetPool.Count > 0)
        {
            if (targetAll)
            {
                foreach (var c in targetPool)
                    if (shown < cursors.Count) PlaceCursor(cursors[shown++], c);
            }
            else
            {
                int i = Mathf.Clamp(targetIndex, 0, targetPool.Count - 1);
                PlaceCursor(cursors[shown++], targetPool[i]);
            }
        }

        for (int i = 0; i < cursors.Count; i++)
            cursors[i].gameObject.SetActive(i < shown);
    }

    void PlaceCursor(Image cursor, Combatant c)
    {
        var cam = Cam;
        if (cam == null) return;
        cursor.rectTransform.position = cam.WorldToScreenPoint(TopOf(c));
    }

    Vector3 TopOf(Combatant c)
    {
        var sr = c.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            return new Vector3(sr.bounds.center.x, sr.bounds.max.y + 0.2f, c.transform.position.z);
        return c.transform.position + Vector3.up * 2f;
    }

    // ---------- eventos del combate ----------

    void OnWaveStarted(int wave) => RebuildEnemyLabels();

    void OnStateChanged(BattleState s)
    {
        bool end = s == BattleState.Victory || s == BattleState.Defeat;
        bigText.gameObject.SetActive(end);
        if (end) bigText.text = s == BattleState.Victory ? "¡VICTORIA!" : "DERROTA";
    }

    void ShowMessage(string msg)
    {
        messageText.text = msg;
        messageWindow.gameObject.SetActive(true);
        messageTimer = 1.8f;
    }

    void ShowNumber(Combatant target, int amount, bool heal)
    {
        StartCoroutine(NumberRoutine(target, amount, heal));
    }

    IEnumerator NumberRoutine(Combatant target, int amount, bool heal)
    {
        var t = MakeText("Numero", root, amount.ToString(), theme.fontSize + 16,
                         TextAlignmentOptions.Midline, heal ? theme.healColor : theme.damageColor);
        t.fontStyle = FontStyles.Bold;
        var rt = t.rectTransform;
        rt.sizeDelta = new Vector2(240f, 70f);

        Vector3 startWorld = TopOf(target);
        float time = 0f;

        while (time < 0.9f)
        {
            time += Time.deltaTime;
            var cam = Cam;
            if (cam == null) break;

            rt.position = cam.WorldToScreenPoint(startWorld) + Vector3.up * (time * 90f);

            var col = t.color;
            col.a = 1f - Mathf.Clamp01((time - 0.5f) / 0.4f);
            t.color = col;
            yield return null;
        }
        Destroy(t.gameObject);
    }

    // ---------- menús ----------

    void ManageMenu()
    {
        if (mode != Mode.None)
        {
            bool over = battle.State == BattleState.Starting || battle.State == BattleState.WaveCleared
                     || battle.State == BattleState.Victory || battle.State == BattleState.Defeat;
            if (over || actor == null || !actor.IsReady || actor.commandQueued)
                CloseMenu();
        }

        if (mode == Mode.None && battle.State == BattleState.Running)
        {
            var p = battle.NextReadyPlayer();
            if (p != null) OpenCommandMenu(p);
        }
    }

    void OpenCommandMenu(Combatant p)
    {
        actor = p;
        commandList.entries.Clear();

        if (p.limitBreak != null && p.limit >= 1f)
            commandList.entries.Add(new MenuEntry { label = "Límite", id = CommandId.Limit, action = p.limitBreak });

        commandList.entries.Add(new MenuEntry
        {
            label = "Ataque",
            id = CommandId.Attack,
            action = p.basicAttack,
            enabled = p.basicAttack != null
        });

        if (p.magics != null && p.magics.Length > 0)
            commandList.entries.Add(new MenuEntry { label = "Magia", id = CommandId.Magic });

        commandList.entries.Add(new MenuEntry { label = "Defender", id = CommandId.Defend });

        commandList.selected = 0;
        commandList.scroll = 0;
        commandList.title.text = p.displayName;
        RefreshList(commandList);

        commandList.window.gameObject.SetActive(true);
        magicList.window.gameObject.SetActive(false);
        mode = Mode.Command;
    }

    void OpenMagicMenu()
    {
        magicList.entries.Clear();
        foreach (var a in actor.magics)
        {
            if (a == null) continue;
            magicList.entries.Add(new MenuEntry
            {
                label = a.displayName,
                right = a.mpCost + " MP",
                enabled = battle.CanUse(actor, a),
                id = CommandId.Spell,
                action = a
            });
        }

        magicList.selected = 0;
        magicList.scroll = 0;
        magicList.title.text = "Magia";
        RefreshList(magicList);

        magicList.window.gameObject.SetActive(true);
        mode = Mode.Magic;
    }

    void CloseMenu()
    {
        mode = Mode.None;
        actor = null;
        commandList.window.gameObject.SetActive(false);
        magicList.window.gameObject.SetActive(false);
        targetWindow.gameObject.SetActive(false);
    }

    void RefreshList(MenuList l)
    {
        for (int i = 0; i < l.Rows; i++)
        {
            int idx = l.scroll + i;
            if (idx >= l.entries.Count)
            {
                l.left[i].text = "";
                l.right[i].text = "";
                continue;
            }

            var e = l.entries[idx];
            bool selected = idx == l.selected;
            Color c = !e.enabled ? theme.disabledColor : selected ? theme.selectedColor : theme.textColor;

            l.left[i].text = (selected ? "> " : "    ") + e.label;
            l.left[i].color = c;
            l.right[i].text = e.right ?? "";
            l.right[i].color = c;
        }
    }

    void MoveSelection(MenuList l, int dir)
    {
        if (l.entries.Count == 0) return;

        l.selected = (l.selected + dir + l.entries.Count) % l.entries.Count;
        if (l.selected < l.scroll) l.scroll = l.selected;
        if (l.selected >= l.scroll + l.Rows) l.scroll = l.selected - l.Rows + 1;
        RefreshList(l);
    }

    void ConfirmCommand()
    {
        if (commandList.entries.Count == 0) return;
        var e = commandList.entries[commandList.selected];
        if (!e.enabled) return;

        switch (e.id)
        {
            case CommandId.Defend:
                battle.QueueDefend(actor);
                CloseMenu();
                break;
            case CommandId.Attack:
            case CommandId.Limit:
                BeginTarget(e.action, Mode.Command);
                break;
            case CommandId.Magic:
                OpenMagicMenu();
                break;
        }
    }

    void ConfirmMagic()
    {
        if (magicList.entries.Count == 0) return;
        var e = magicList.entries[magicList.selected];
        if (!e.enabled) return;
        BeginTarget(e.action, Mode.Magic);
    }

    // ---------- selección de objetivo ----------

    void BeginTarget(ActionData a, Mode returnTo)
    {
        var pool = battle.GetValidTargets(actor, a);
        if (pool.Count == 0) return;

        pendingAction = a;
        targetReturn = returnTo;
        targetPool = pool;
        targetAll = a.target == TargetType.AllEnemies || a.target == TargetType.AllAllies;
        targetIndex = 0;

        // para curaciones, arrancar apuntando al propio personaje
        if (a.target == TargetType.SingleAlly)
        {
            int self = targetPool.IndexOf(actor);
            if (self >= 0) targetIndex = self;
        }

        mode = Mode.Target;
        targetWindow.gameObject.SetActive(true);
        UpdateTargetText();
    }

    void UpdateTargetText()
    {
        if (targetAll)
            targetText.text = pendingAction.target == TargetType.AllEnemies ? "Todos los enemigos" : "Todo el grupo";
        else
            targetText.text = targetPool[targetIndex].displayName;
    }

    void ExitTarget()
    {
        targetWindow.gameObject.SetActive(false);
        mode = targetReturn;
    }

    void ConfirmTarget()
    {
        var targets = targetAll
            ? new List<Combatant>(targetPool)
            : new List<Combatant> { targetPool[targetIndex] };

        battle.QueueCommand(actor, pendingAction, targets);
        CloseMenu();
    }

    // ---------- teclado ----------

    void HandleInput()
    {
        var kb = Keyboard.current;
        if (kb == null || mode == Mode.None) return;

        bool goUp = kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
        bool goDown = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
        bool goLeft = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
        bool goRight = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
        bool pressOk = kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.zKey.wasPressedThisFrame;
        bool pressBack = kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame;

        switch (mode)
        {
            case Mode.Command:
                if (goUp) MoveSelection(commandList, -1);
                else if (goDown) MoveSelection(commandList, 1);
                else if (pressOk) ConfirmCommand();
                break;

            case Mode.Magic:
                if (goUp) MoveSelection(magicList, -1);
                else if (goDown) MoveSelection(magicList, 1);
                else if (pressOk) ConfirmMagic();
                else if (pressBack)
                {
                    magicList.window.gameObject.SetActive(false);
                    mode = Mode.Command;
                }
                break;

            case Mode.Target:
                targetPool.RemoveAll(c => c == null || c.IsDead);
                if (targetPool.Count == 0) { ExitTarget(); break; }
                targetIndex = Mathf.Clamp(targetIndex, 0, targetPool.Count - 1);

                if (goLeft || goUp)
                {
                    targetIndex = (targetIndex - 1 + targetPool.Count) % targetPool.Count;
                    UpdateTargetText();
                }
                else if (goRight || goDown)
                {
                    targetIndex = (targetIndex + 1) % targetPool.Count;
                    UpdateTargetText();
                }
                else if (pressOk) ConfirmTarget();
                else if (pressBack) ExitTarget();
                break;
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

    static void PlaceTopLeft(RectTransform rt, float x, float y, float w, float h)
    {
        Place(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -y), new Vector2(w, h));
    }

    // ocupa todo el ancho de la ventana, con un margen a cada lado
    static void PlaceTopStretch(RectTransform rt, float margin, float y, float h)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -y);
        rt.sizeDelta = new Vector2(-2f * margin, h);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetFill(RectTransform fill, float value)
    {
        fill.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
    }

    RectTransform MakeWindow(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Place(rt, anchor, pivot, pos, size);

        var img = go.GetComponent<Image>();
        img.color = theme.windowColor;
        img.raycastTarget = false;

        var outline = go.GetComponent<Outline>();
        outline.effectColor = theme.borderColor;
        outline.effectDistance = new Vector2(3f, -3f);
        return rt;
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

    void MakeBar(Transform parent, float x, float y, float w, float h, out RectTransform fill, out Image fillImage)
    {
        var bg = new GameObject("BarraFondo", typeof(RectTransform), typeof(Image));
        var bgRt = (RectTransform)bg.transform;
        bgRt.SetParent(parent, false);
        PlaceTopLeft(bgRt, x, y, w, h);

        var bgImg = bg.GetComponent<Image>();
        bgImg.color = theme.barBackColor;
        bgImg.raycastTarget = false;

        var f = new GameObject("Relleno", typeof(RectTransform), typeof(Image));
        fill = (RectTransform)f.transform;
        fill.SetParent(bgRt, false);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0f, 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        fillImage = f.GetComponent<Image>();
        fillImage.color = theme.atbColor;
        fillImage.raycastTarget = false;
    }
}