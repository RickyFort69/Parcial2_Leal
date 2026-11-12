using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BattleState { Starting, Running, Acting, WaveCleared, Victory, Defeat }

public class BattleCommand
{
    public Combatant user;
    public ActionData action;                  // null si es defender
    public List<Combatant> targets = new List<Combatant>();
    public bool defend;
}

public class BattleManager : MonoBehaviour
{
    [Header("Datos")]
    public BattleConfig config;
    public CharacterData[] partyData;

    [Header("Escena")]
    public Combatant combatantPrefab;
    public Transform[] partySlots;
    public Transform[] enemySlots;

    [Header("Depuración (temporal, hasta tener el menú)")]
    public bool debugKeys = true;

    public readonly List<Combatant> party = new List<Combatant>();
    public readonly List<Combatant> enemies = new List<Combatant>();

    public BattleState State { get; private set; } = BattleState.Starting;
    public int CurrentWave { get; private set; }
    public bool MenuOpen { get; set; }          // la interfaz lo pondrá en true mientras elegís

    public event System.Action<int> OnWaveStarted;
    public event System.Action<BattleState> OnStateChanged;
    public event System.Action<string> OnMessage;
    public event System.Action<Combatant, int, bool> OnNumber;   // objetivo, cantidad, ¿es curación?

    readonly Queue<BattleCommand> queue = new Queue<BattleCommand>();
    string lastMessage = "";

    // ---------- Inicio ----------

    void Start()
    {
        if (config == null || config.waves == null || config.waves.Length == 0)
        {
            Debug.LogError("BattleManager: falta el BattleConfig o no tiene oleadas", this);
            enabled = false;
            return;
        }
        if (partyData == null || partyData.Length == 0 || combatantPrefab == null)
        {
            Debug.LogError("BattleManager: falta el grupo (Party Data) o el Combatant Prefab", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < partyData.Length && i < partySlots.Length; i++)
        {
            var c = Instantiate(combatantPrefab, partySlots[i].position, Quaternion.identity);
            c.Setup(partyData[i]);
            party.Add(c);
        }

        StartCoroutine(WaveLoop());
    }

    // ---------- Oleadas ----------

    IEnumerator WaveLoop()
    {
        for (int i = 0; i < config.waves.Length; i++)
        {
            CurrentWave = i + 1;
            SpawnWave(config.waves[i]);
            SetState(BattleState.Starting);
            OnWaveStarted?.Invoke(CurrentWave);
            Say($"¡Oleada {CurrentWave}!");
            yield return new WaitForSeconds(1f);

            SetState(BattleState.Running);
            while (State != BattleState.WaveCleared && State != BattleState.Defeat)
                yield return null;

            if (State == BattleState.Defeat) yield break;

            Say("¡Oleada superada!");
            yield return new WaitForSeconds(1.2f);

            bool last = i == config.waves.Length - 1;
            if (!last && config.recoverEveryWaves > 0 && CurrentWave % config.recoverEveryWaves == 0)
            {
                Recover();
                yield return new WaitForSeconds(1.2f);
            }
        }

        Say("¡Victoria!");
        SetState(BattleState.Victory);
    }

    void SpawnWave(WaveData wave)
    {
        foreach (var e in enemies) if (e != null) Destroy(e.gameObject);
        enemies.Clear();
        queue.Clear();

        for (int i = 0; i < wave.enemies.Length && i < enemySlots.Length; i++)
        {
            var c = Instantiate(combatantPrefab, enemySlots[i].position, Quaternion.identity);
            c.Setup(wave.enemies[i]);
            c.atb = Random.Range(0f, 0.5f);
            enemies.Add(c);
        }

        foreach (var p in party)
        {
            p.atb = Random.Range(0f, 0.4f);
            p.commandQueued = false;
            p.defending = false;
        }
    }

    void Recover()
    {
        foreach (var p in party)
        {
            int hpGain = Mathf.RoundToInt(p.maxHP * config.recoverHPPercent);
            int mpGain = Mathf.RoundToInt(p.maxMP * config.recoverMPPercent);

            if (p.IsDead) p.Revive(Mathf.Max(1, hpGain));
            else p.Heal(hpGain);
            p.RestoreMP(mpGain);

            OnNumber?.Invoke(p, hpGain, true);
        }
        Say("¡El grupo recupera fuerzas!");
    }

    // ---------- Bucle principal ----------

    void Update()
    {
        if (State != BattleState.Running) return;

        bool paused = config.waitWhileMenuOpen && MenuOpen;
        if (!paused) TickATB(Time.deltaTime);

        // los enemigos listos eligen su acción
        foreach (var e in enemies)
            if (e.IsReady && !e.commandQueued) QueueEnemyCommand(e);

        if (debugKeys) HandleDebugKeys();

        if (queue.Count > 0)
            StartCoroutine(ExecuteCommand(queue.Dequeue()));
    }

    void TickATB(float dt)
    {
        foreach (var c in party) TickOne(c, dt);
        foreach (var c in enemies) TickOne(c, dt);
    }

    void TickOne(Combatant c, float dt)
    {
        if (c.IsDead || c.atb >= 1f) return;
        c.atb = Mathf.Min(1f, c.atb + (c.speed / 100f) * config.atbSpeedMultiplier * dt);
    }

    // ---------- Comandos (los usa la interfaz) ----------

    public bool CanUse(Combatant user, ActionData a)
    {
        if (a == null) return false;
        if (a.kind == ActionKind.Limit) return user.limit >= 1f;
        return user.mp >= a.mpCost;
    }

    public void QueueCommand(Combatant user, ActionData action, List<Combatant> targets)
    {
        if (user.commandQueued || targets.Count == 0) return;
        user.commandQueued = true;
        queue.Enqueue(new BattleCommand { user = user, action = action, targets = targets });
    }

    public void QueueDefend(Combatant user)
    {
        if (user.commandQueued) return;
        user.commandQueued = true;
        queue.Enqueue(new BattleCommand { user = user, defend = true });
    }

    public Combatant NextReadyPlayer() => party.Find(c => c.IsReady && !c.commandQueued);

    // Devuelve los objetivos posibles. 'chosen' es el elegido por el jugador (puede ser null = al azar).
    public List<Combatant> ResolveTargets(Combatant user, ActionData a, Combatant chosen)
    {
        bool targetsRivals = a.target == TargetType.SingleEnemy || a.target == TargetType.AllEnemies;
        List<Combatant> pool = (targetsRivals == user.isPlayer) ? enemies : party;

        var alive = pool.FindAll(c => !c.IsDead);
        var result = new List<Combatant>();
        if (alive.Count == 0) return result;

        if (a.target == TargetType.AllEnemies || a.target == TargetType.AllAllies) return alive;

        if (chosen != null && alive.Contains(chosen)) result.Add(chosen);
        else result.Add(alive[Random.Range(0, alive.Count)]);
        return result;
    }

    // ---------- Enemigos ----------

    void QueueEnemyCommand(Combatant e)
    {
        if (e.enemyActions == null || e.enemyActions.Length == 0) { e.atb = 0f; return; }

        var action = e.enemyActions[Random.Range(0, e.enemyActions.Length)];
        var targets = ResolveTargets(e, action, null);
        if (targets.Count == 0) { e.atb = 0f; return; }

        QueueCommand(e, action, targets);
    }

    // ---------- Ejecución ----------

    IEnumerator ExecuteCommand(BattleCommand cmd)
    {
        SetState(BattleState.Acting);
        var user = cmd.user;

        if (!user.IsDead)
        {
            user.defending = false;

            if (cmd.defend)
            {
                user.defending = true;
                Say($"{user.displayName} se defiende");
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                var a = cmd.action;

                if (user.isPlayer)
                {
                    user.mp = Mathf.Max(0, user.mp - a.mpCost);
                    if (a.kind == ActionKind.Limit) user.limit = 0f;
                }

                Say($"{user.displayName}: {a.displayName}");

                var targets = ResolveTargets(user, a, cmd.targets.Count > 0 ? cmd.targets[0] : null);

                if (a.effect == EffectType.Damage && !a.usesMagic && targets.Count > 0)
                    yield return StartCoroutine(Bump(user, targets[0].transform.position));
                else
                    yield return user.Flash(Color.cyan, 0.3f);

                foreach (var t in targets) ApplyAction(user, a, t);
                yield return new WaitForSeconds(0.6f);
            }
        }

        user.atb = 0f;
        user.commandQueued = false;

        if (AllDead(party))
        {
            Say("El grupo ha caído...");
            SetState(BattleState.Defeat);
        }
        else if (AllDead(enemies)) SetState(BattleState.WaveCleared);
        else SetState(BattleState.Running);
    }

    void ApplyAction(Combatant user, ActionData a, Combatant t)
    {
        switch (a.effect)
        {
            case EffectType.Damage:
                {
                    int dmg = CalcDamage(user, a, t);
                    t.TakeDamage(dmg);
                    StartCoroutine(t.Flash(Color.red, 0.15f));
                    OnNumber?.Invoke(t, dmg, false);
                    if (t.isPlayer && !t.IsDead)
                        t.AddLimit((float)dmg / t.maxHP * config.limitGainFactor);
                    break;
                }
            case EffectType.HealHP:
                {
                    int amount = HealAmount(user, a);
                    t.Heal(amount);
                    StartCoroutine(t.Flash(Color.green, 0.25f));
                    OnNumber?.Invoke(t, amount, true);
                    break;
                }
            case EffectType.HealMP:
                {
                    int amount = HealAmount(user, a);
                    t.RestoreMP(amount);
                    StartCoroutine(t.Flash(Color.blue, 0.25f));
                    OnNumber?.Invoke(t, amount, true);
                    break;
                }
        }
    }

    int CalcDamage(Combatant user, ActionData a, Combatant target)
    {
        float atkStat = a.usesMagic ? user.mag : user.atk;
        float defStat = a.usesMagic ? target.mdef : target.def;

        float raw = atkStat * a.power * config.statMultiplier - defStat;
        if (target.defending) raw *= config.defendReduction;
        raw *= 1f + Random.Range(-config.variance, config.variance);

        return Mathf.Max(1, Mathf.RoundToInt(raw));
    }

    int HealAmount(Combatant user, ActionData a)
    {
        if (a.fixedAmount > 0) return a.fixedAmount;
        return Mathf.RoundToInt(user.mag * a.power * config.statMultiplier);
    }

    // ---------- Animaciones simples ----------

    IEnumerator Bump(Combatant user, Vector3 target)
    {
        Vector3 start = user.transform.position;
        Vector3 end = Vector3.Lerp(start, target, 0.75f);
        yield return StartCoroutine(MoveTo(user.transform, start, end, 0.18f));
        yield return StartCoroutine(MoveTo(user.transform, end, start, 0.18f));
    }

    IEnumerator MoveTo(Transform t, Vector3 from, Vector3 to, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            t.position = Vector3.Lerp(from, to, elapsed / time);
            yield return null;
        }
        t.position = to;
    }

    // ---------- Utilidades ----------

    static bool AllDead(List<Combatant> list)
    {
        foreach (var c in list) if (!c.IsDead) return false;
        return true;
    }

    void SetState(BattleState s)
    {
        State = s;
        OnStateChanged?.Invoke(s);
    }

    void Say(string msg)
    {
        lastMessage = msg;
        OnMessage?.Invoke(msg);
        Debug.Log(msg);
    }
    public List<Combatant> GetValidTargets(Combatant user, ActionData a)
    {
        bool targetsRivals = a.target == TargetType.SingleEnemy || a.target == TargetType.AllEnemies;
        List<Combatant> pool = (targetsRivals == user.isPlayer) ? enemies : party;
        return pool.FindAll(c => !c.IsDead);
    }

    // ---------- Depuración (se borra cuando esté el menú) ----------

    void HandleDebugKeys()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        var p = NextReadyPlayer();
        if (p == null) return;

        if (kb.digit1Key.wasPressedThisFrame && p.basicAttack != null)
            QueueCommand(p, p.basicAttack, ResolveTargets(p, p.basicAttack, null));
        else if (kb.digit2Key.wasPressedThisFrame && p.magics != null && p.magics.Length > 0 && CanUse(p, p.magics[0]))
            QueueCommand(p, p.magics[0], ResolveTargets(p, p.magics[0], null));
        else if (kb.digit3Key.wasPressedThisFrame)
            QueueDefend(p);
        else if (kb.digit4Key.wasPressedThisFrame && p.limitBreak != null && CanUse(p, p.limitBreak))
            QueueCommand(p, p.limitBreak, ResolveTargets(p, p.limitBreak, null));
    }

    void OnGUI()
    {
        if (!debugKeys) return;

        float y = 10f;
        GUI.Label(new Rect(10, y, 700, 22), $"Oleada {CurrentWave}  |  {State}"); y += 24;

        foreach (var c in party)
        {
            string ready = c.IsReady ? "   <LISTO>" : "";
            GUI.Label(new Rect(10, y, 800, 22),
                $"{c.displayName}   HP {c.hp}/{c.maxHP}   MP {c.mp}/{c.maxMP}   ATB {(int)(c.atb * 100)}%   LÍMITE {(int)(c.limit * 100)}%{ready}");
            y += 22;
        }

        y += 8;
        foreach (var e in enemies)
        {
            GUI.Label(new Rect(10, y, 800, 22), $"{e.displayName}   HP {e.hp}/{e.maxHP}   ATB {(int)(e.atb * 100)}%");
            y += 22;
        }

        y += 8;
        GUI.Label(new Rect(10, y, 800, 22), lastMessage); y += 24;
        GUI.Label(new Rect(10, y, 800, 22), "Teclas (actúa el primer personaje LISTO):  1 Ataque   2 Magia   3 Defender   4 Límite");
    }
}