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


    // =========================================================
    // INITIALIZATION
    // =========================================================

    public void Initialize(
        Vector3 shotDirection,
        float projectileSpeed,
        float projectileDamage,
        float projectileMaxRange,
        LayerMask projectileHitMask)
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
        // Move projectile exactly to collision point.
        transform.position =
            hit.point;


        // =========================================================
        // DAMAGE
        // =========================================================

        IDamageable damageable =
            hit.collider
                .GetComponentInParent<IDamageable>();


        if (damageable != null)
        {
            damageable.TakeDamage(
                damage
            );
        }


        // =========================================================
        // DEBUG
        // =========================================================

        Debug.Log(
            $"[Projectile] Hit: {hit.collider.name} " +
            $"| Damage: {damage}",
            hit.collider
        );


        // =========================================================
        // DESTROY PROJECTILE
        // =========================================================

        Destroy(
            gameObject
        );
    }
}
