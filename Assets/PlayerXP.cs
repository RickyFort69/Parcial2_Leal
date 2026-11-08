using System;
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    public float pickupRange = 3f;
    public LayerMask pickupMask;
    public int level = 1;
    public int currentXP;
    public int xpToNext = 5;

    public event Action<int, int> OnXPChanged;
    public event Action<int> OnLevelUp;

    readonly Collider[] gemBuffer = new Collider[64];

    void Update()
    {
        int n = Physics.OverlapSphereNonAlloc(transform.position, pickupRange, gemBuffer, pickupMask);
        for (int i = 0; i < n; i++)
            if (gemBuffer[i].TryGetComponent<XPGem>(out var gem)) gem.Attract(transform);
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            level++;
            xpToNext = Mathf.RoundToInt(xpToNext * 1.3f + 2);
            GetComponent<Health>().IncreaseMax(10f); // más vida al subir
            OnLevelUp?.Invoke(level);
        }
        OnXPChanged?.Invoke(currentXP, xpToNext);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}