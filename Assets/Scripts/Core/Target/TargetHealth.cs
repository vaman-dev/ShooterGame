using System;
using UnityEngine;

public class TargetHealth : MonoBehaviour, IDamageable
{
    // =========================================================
    // HEALTH
    // =========================================================

    [Header("Health")]

    [SerializeField]
    private float maxHealth = 100f;


    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Death")]

    [Tooltip(
        "For the current vertical-slice target, " +
        "destroy the object when health reaches zero."
    )]
    [SerializeField]
    private bool destroyOnDeath = true;


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


        string instigatorName =
            damageInfo.Instigator != null
                ? damageInfo.Instigator.name
                : "Unknown";


        string weaponName =
            damageInfo.WeaponData != null
                ? damageInfo.WeaponData.weaponName
                : "Unknown";


        Debug.Log(
            $"[Health] {name} took {damageInfo.Amount} damage. " +
            $"Health: {CurrentHealth}/{maxHealth} " +
            $"| From: {instigatorName} " +
            $"| Weapon: {weaponName}",
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
            $"[Health] {name} died.",
            this
        );


        Died?.Invoke();


        if (destroyOnDeath)
        {
            Destroy(
                gameObject
            );
        }
    }
}
