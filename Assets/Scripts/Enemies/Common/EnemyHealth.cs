using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField]
    EnemyCombatConfig combatConfig;

    [SerializeField]
    float legacyMaxHealth = 3f;

    public event Action OnDeath;

    public bool IsDead { get; private set; }
    public float CurrentHealth { get; private set; }

    /// <summary>Optional orchestrator hook so combat SO + legacy fallback apply before first damage.</summary>
    public void ApplyBindings(EnemyCombatConfig config, float legacyMax)
    {
        combatConfig = config;
        legacyMaxHealth = legacyMax;
        float max = combatConfig != null ? combatConfig.maxHealth : legacyMaxHealth;
        CurrentHealth = max;
    }

    void Awake()
    {
        if (CurrentHealth <= 0f)
        {
            float max = combatConfig != null ? combatConfig.maxHealth : legacyMaxHealth;
            CurrentHealth = max;
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead)
            return;

        CurrentHealth -= amount;
        if (CurrentHealth <= 0f)
            Die();
    }

    void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        OnDeath?.Invoke();
    }
}
