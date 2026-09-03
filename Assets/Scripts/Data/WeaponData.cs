using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponData",
    menuName = "TPS/Weapons/Weapon Data"
)]
public class WeaponData : ScriptableObject
{
    // =========================================================
    // IDENTITY
    // =========================================================

    [Header("Identity")]

    public string weaponName = "Rifle";


    // =========================================================
    // PROJECTILE
    // =========================================================

    [Header("Projectile")]

    [Tooltip("Projectile prefab spawned from the MuzzlePoint.")]
    public GameObject projectilePrefab;

    [Min(0.1f)]
    public float projectileSpeed = 60f;

    [Min(0f)]
    public float damage = 20f;

    [Min(1f)]
    public float maxRange = 100f;


    // =========================================================
    // AMMO
    // =========================================================

    [Header("Ammo")]

    [Min(1)]
    public int maxAmmo = 30;

    [Min(0.01f)]
    public float reloadDuration = 1.5f;


    // =========================================================
    // FIRING
    // =========================================================

    [Header("Firing")]

    [Tooltip("Seconds between two shots.")]
    [Min(0.01f)]
    public float fireInterval = 0.15f;

    public bool automatic = true;


    // =========================================================
    // ACCURACY
    // =========================================================

    [Header("Accuracy")]

    [Tooltip(
        "Random firing cone in degrees. " +
        "Keep 0 while validating the vertical slice."
    )]
    [Range(0f, 10f)]
    public float spreadAngle = 0f;
}