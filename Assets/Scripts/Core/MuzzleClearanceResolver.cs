using System;
using UnityEngine;

[Serializable]
public sealed class MuzzleClearanceResolver
{
    // The project's current layer layout places cover on layer 7 and
    // environment on layer 9. Targets, players, and projectiles are
    // intentionally excluded from this production-clearance mask.
    private const int DefaultObstructionMask =
        (1 << 7) |
        (1 << 9);

    [Header("Muzzle Clearance")]

    [Tooltip(
        "Geometry that can make the muzzle invalid. " +
        "Use coverLayer and EnvrionmentLayer only."
    )]
    [SerializeField]
    private LayerMask obstructionMask =
        DefaultObstructionMask;

    [Tooltip("Radius used to test the physical space around the muzzle.")]
    [SerializeField]
    [Min(0.001f)]
    private float clearanceRadius = 0.045f;

    [Tooltip("Short distance checked immediately in front of the muzzle.")]
    [SerializeField]
    [Min(0f)]
    private float clearanceDistance = 0.15f;

    [Tooltip("Draw the authoritative clearance result when a shot is attempted.")]
    [SerializeField]
    private bool showDebug = true;


    public bool HasClearance(
        Vector3 muzzlePosition,
        Vector3 fireDirection)
    {
        if (fireDirection.sqrMagnitude < 0.001f)
            return false;


        Vector3 direction =
            fireDirection.normalized;


        // Check 1: reject a muzzle whose clearance volume is already
        // intersecting cover or environment geometry.
        bool overlapsObstruction =
            Physics.CheckSphere(
                muzzlePosition,
                clearanceRadius,
                obstructionMask,
                QueryTriggerInteraction.Ignore
            );


        if (overlapsObstruction)
        {
            DrawClearance(
                muzzlePosition,
                direction,
                false
            );

            return false;
        }


        // Check 2: reject geometry immediately in front of the muzzle.
        // Projectile travel beyond this short clearance distance remains
        // the responsibility of SimpleProjectile.
        bool blockedInFront =
            clearanceDistance > 0f &&
            Physics.SphereCast(
                muzzlePosition,
                clearanceRadius,
                direction,
                out _,
                clearanceDistance,
                obstructionMask,
                QueryTriggerInteraction.Ignore
            );


        DrawClearance(
            muzzlePosition,
            direction,
            !blockedInFront
        );


        return !blockedInFront;
    }


    private void DrawClearance(
        Vector3 muzzlePosition,
        Vector3 direction,
        bool isClear)
    {
        if (!showDebug)
            return;


        Debug.DrawRay(
            muzzlePosition,
            direction * clearanceDistance,
            isClear
                ? Color.green
                : Color.red,
            0.25f
        );
    }
}
