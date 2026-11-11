using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Combatant : MonoBehaviour
{
    public bool isPlayer;
    public string displayName;

    [Header("Stats actuales")]
    public int maxHP, hp, maxMP, mp;
    public int atk, mag, def, mdef, speed;

    [Header("Barras (0 a 1)")]
    [Range(0f, 1f)] public float atb;
    [Range(0f, 1f)] public float limit;

    [Header("Acciones")]
    public ActionData basicAttack;
    public ActionData[] magics;
    public ActionData limitBreak;
    public ActionData[] enemyActions;

    [HideInInspector] public bool commandQueued;
    [HideInInspector] public bool defending;

    SpriteRenderer sr;

    public bool IsDead => hp <= 0;
    public bool IsReady => !IsDead && atb >= 1f;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Setup(CharacterData d)
    {
        isPlayer = true;
        displayName = d.displayName;
        maxHP = hp = d.maxHP;
        maxMP = mp = d.maxMP;
        atk = d.attack; mag = d.magic; def = d.defense; mdef = d.magicDefense; speed = d.speed;
        basicAttack = d.basicAttack; magics = d.magics; limitBreak = d.limitBreak;
        SetSprite(d.sprite, d.flipX);
    }

    public void Setup(FFEnemyData d)
    {
        isPlayer = false;
        displayName = d.displayName;
        maxHP = hp = d.maxHP;
        maxMP = mp = 0;
        atk = d.attack; mag = d.magic; def = d.defense; mdef = d.magicDefense; speed = d.speed;
        enemyActions = d.actions;
        SetSprite(d.sprite, d.flipX);
    }

    void SetSprite(Sprite s, bool flip)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.flipX = flip;
        sr.color = Color.white;
    }

    public void TakeDamage(int amount)
    {
        bool wasAlive = !IsDead;
        hp = Mathf.Max(0, hp - Mathf.Max(0, amount));
        if (wasAlive && IsDead) OnDeath();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        hp = Mathf.Min(maxHP, hp + Mathf.Max(0, amount));
    }

    public void RestoreMP(int amount)
    {
        if (IsDead) return;
        mp = Mathf.Min(maxMP, mp + Mathf.Max(0, amount));
    }

    public void AddLimit(float amount) => limit = Mathf.Clamp01(limit + amount);

    public void Revive(int amount)
    {
        hp = Mathf.Clamp(amount, 1, maxHP);
        sr.color = Color.white;
    }

    void OnDeath()
    {
        atb = 0f;
        if (isPlayer) sr.color = new Color(0.4f, 0.4f, 0.4f, 1f);   // aliado caído: gris
        else StartCoroutine(FadeOut());                              // enemigo: desaparece
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.2f);
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            sr.color = new Color(1f, 1f, 1f, 1f - t / 0.5f);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    public IEnumerator Flash(Color color, float time)
    {
        if (sr == null) yield break;
        sr.color = color;
        yield return new WaitForSeconds(time);
        if (!IsDead) sr.color = Color.white;
    }
}