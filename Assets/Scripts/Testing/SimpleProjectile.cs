using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    // =========================================================
    // RUNTIME
    // =========================================================

    private Vector3 direction;

    private float speed;

    private float damage;

    private float maxRange;

    private float travelledDistance;

    private LayerMask hitMask;

    private bool initialized;

    private GameObject instigator;
    private WeaponData sourceWeapon;

    public GameObject Instigator => instigator;
    public WeaponData SourceWeapon => sourceWeapon;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    public void Initialize(
        Vector3 shotDirection,
        float projectileSpeed,
        float projectileDamage,
        float projectileMaxRange,
        LayerMask projectileHitMask,
        GameObject projectileInstigator,
        WeaponData projectileWeapon)
    {
        direction =
            shotDirection.normalized;

        speed =
            projectileSpeed;

        damage =
            projectileDamage;

        maxRange =
            projectileMaxRange;

        hitMask =
            projectileHitMask;

        instigator =
            projectileInstigator;

        sourceWeapon =
            projectileWeapon;

        travelledDistance =
            0f;

        initialized =
            true;
    }


    // =========================================================
    // MOVEMENT
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;


        float remainingDistance =
            maxRange -
            travelledDistance;


        if (remainingDistance <= 0f)
        {
            Destroy(
                gameObject
            );

            return;
        }


        float distanceThisFrame =
            speed *
            Time.deltaTime;


        float movementDistance =
            Mathf.Min(
                distanceThisFrame,
                remainingDistance
            );


        // Raycast before moving.
        // This prevents fast projectiles tunnelling through
        // thin geometry.

        if (Physics.Raycast(
            transform.position,
            direction,
            out RaycastHit hit,
            movementDistance,
            hitMask,
            QueryTriggerInteraction.Ignore))
        {
            HandleHit(
                hit
            );

            return;
        }


        transform.position +=
            direction *
            movementDistance;


        travelledDistance +=
            movementDistance;


        if (travelledDistance >= maxRange)
        {
            Destroy(
                gameObject
            );
        }
    }


    // =========================================================
    // HIT
    // =========================================================

    private void HandleHit(
        RaycastHit hit)
    {
        transform.position =
            hit.point;


        // =========================================================
        // DEFENSIVE OWNER CHECK
        // =========================================================

        if (instigator != null &&
            hit.collider.transform.root ==
            instigator.transform.root)
        {
            // Fail safely if layer filtering ever allows the projectile
            // to encounter its owner. Do not leave a projectile stuck
            // repeatedly resolving the same owner hit.
            Destroy(
                gameObject
            );

            return;
        }


        // =========================================================
        // DAMAGE
        // =========================================================

        IDamageable damageable =
            hit.collider
                .GetComponentInParent<IDamageable>();


        if (damageable != null)
        {
            DamageInfo damageInfo =
                new DamageInfo(
                    damage,
                    hit.point,
                    hit.normal,
                    instigator,
                    sourceWeapon
                );


            damageable.TakeDamage(
                damageInfo
            );
        }


        // =========================================================
        // DEBUG
        // =========================================================

        Debug.Log(
            $"[Projectile] Hit: {hit.collider.name} " +
            $"| Damage: {damage} " +
            $"| Instigator: {(instigator != null ? instigator.name : "None")} " +
            $"| Weapon: {(sourceWeapon != null ? sourceWeapon.weaponName : "None")}",
            hit.collider
        );


        Destroy(
            gameObject
        );
    }
}
