using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    // =========================================================
    // HEALTH
    // =========================================================

    [Header("Health")]

    [SerializeField]
    [Min(1f)]
    private float maxHealth = 100f;


    // =========================================================
    // EVENTS
    // =========================================================

    public event Action<float, float> HealthChanged;

    public event Action Died;


    // =========================================================
    // RUNTIME
    // =========================================================

    public float CurrentHealth
    {
        get;
        private set;
    }


    public float MaxHealth =>
        maxHealth;


    public bool IsDead
    {
        get;
        private set;
    }


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        CurrentHealth =
            maxHealth;
    }


    // =========================================================
    // DAMAGE
    // =========================================================

    public void TakeDamage(
        DamageInfo damageInfo)
    {
        if (IsDead)
            return;


        if (damageInfo.Amount <= 0f)
            return;


        CurrentHealth =
            Mathf.Max(
                0f,
                CurrentHealth -
                damageInfo.Amount
            );


        Debug.Log(
            $"[PlayerHealth] Took {damageInfo.Amount} damage. " +
            $"Health: {CurrentHealth}/{maxHealth}",
            this
        );


        HealthChanged?.Invoke(
            CurrentHealth,
            maxHealth
        );


        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }


#if UNITY_EDITOR

    [ContextMenu("DEBUG/Apply 25 Damage")]
    private void DebugApply25Damage()
    {
        DamageInfo debugDamage =
            new DamageInfo(
                25f,
                transform.position,
                Vector3.up,
                null,
                null
            );


        TakeDamage(
            debugDamage
        );
    }

#endif


    // =========================================================
    // DEATH
    // =========================================================

    private void Die()
    {
        if (IsDead)
            return;


        IsDead =
            true;


        Debug.Log(
            "[PlayerHealth] Player died.",
            this
        );


        Died?.Invoke();
    }
}
