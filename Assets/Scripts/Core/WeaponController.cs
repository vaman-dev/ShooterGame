//using UnityEngine;

//public class WeaponController : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private PlayerInputReader inputReader;
//    [SerializeField] private AimController aimController;

//    [Header("Weapon Data")]
//    [SerializeField] private int maxAmmo = 30;
//    [SerializeField] private float fireRate = 0.15f; // seconds between shots
//    [SerializeField] private float reloadDuration = 1.5f;

//    private float fireCooldownTimer;
//    private IWeaponState currentState;

//    public WeaponIdleState IdleState { get; private set; }
//    public WeaponFiringState FiringState { get; private set; }
//    public WeaponReloadingState ReloadingState { get; private set; }

//    public PlayerInputReader InputReader => inputReader;
//    public int CurrentAmmo { get; private set; }
//    public float ReloadDuration => reloadDuration;

//    public bool CanFire => CurrentAmmo > 0 && FireCooldownElapsed;
//    public bool CanReload => CurrentAmmo < maxAmmo;
//    public bool FireCooldownElapsed => fireCooldownTimer <= 0f;

//    private void Awake()
//    {
//        IdleState = new WeaponIdleState();
//        FiringState = new WeaponFiringState();
//        ReloadingState = new WeaponReloadingState();

//        CurrentAmmo = maxAmmo;
//    }

//    private void Start()
//    {
//        ChangeState(IdleState);
//    }

//    private void Update()
//    {
//        if (fireCooldownTimer > 0f)
//            fireCooldownTimer -= Time.deltaTime;

//        currentState?.Tick(this);
//    }

//    public void ChangeState(IWeaponState nextState)
//    {
//        if (nextState == null || ReferenceEquals(currentState, nextState))
//            return;

//        if (currentState != null && !currentState.CanTransitionTo(nextState))
//        {
//            Debug.Log($"[Weapon FSM] Blocked transition: {currentState.GetType().Name} → {nextState.GetType().Name}");
//            return;
//        }

//        currentState?.Exit(this);
//        currentState = nextState;
//        currentState.Enter(this);
//    }

//    public void FireShot()
//    {
//        CurrentAmmo--;
//        fireCooldownTimer = fireRate;

//        Vector3 aimPoint = aimController.CurrentAimPoint;
//        Vector3 aimDirection = aimController.CurrentAimDirection;

//        Debug.Log($"[Weapon] Fired shot toward {aimPoint} — Ammo remaining: {CurrentAmmo}");

//        // Actual raycast/hit-detection or projectile spawn goes here next.
//    }

//    public void CompleteReload()
//    {
//        CurrentAmmo = maxAmmo;
//        Debug.Log("[Weapon] Reload complete.");
//    }
//}


using System;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private PlayerInputReader inputReader;

    [SerializeField]
    private AimController aimController;

    [SerializeField]
    private WeaponData weaponData;

    [Tooltip(
        "Actual position from which the projectile leaves the weapon."
    )]
    [SerializeField]
    private Transform muzzlePoint;


    // =========================================================
    // COLLISION
    // =========================================================

    [Header("Hit Detection")]

    [Tooltip(
        "EnvironmentLayer + coverLayer + TargetLayer. " +
        "Exclude Player and ProjectileLayer."
    )]
    [SerializeField]
    private LayerMask projectileHitMask;


    // =========================================================
    // DEBUG
    // =========================================================

    [Header("Debug")]

    [SerializeField]
    private bool showShotDebug = true;


    // =========================================================
    // EVENTS
    // =========================================================

    public event Action ShotFired;

    public event Action<int, int> AmmoChanged;

    public event Action ReloadStarted;

    public event Action ReloadCompleted;

    public event Action FireBlockedNoAmmo;


    // =========================================================
    // RUNTIME
    // =========================================================

    private float nextFireTime;

    private IWeaponState currentState;


    // =========================================================
    // STATES
    // =========================================================

    public WeaponIdleState IdleState
    {
        get;
        private set;
    }

    public WeaponFiringState FiringState
    {
        get;
        private set;
    }

    public WeaponReloadingState ReloadingState
    {
        get;
        private set;
    }


    // =========================================================
    // PUBLIC DATA
    // =========================================================

    public PlayerInputReader InputReader =>
        inputReader;


    public WeaponData Data =>
        weaponData;


    public bool IsEmpty =>
        CurrentAmmo <= 0;


    public bool IsAutomatic =>
        weaponData != null &&
        weaponData.automatic;


    public int CurrentAmmo
    {
        get;
        private set;
    }


    public float ReloadDuration =>
        weaponData != null
            ? weaponData.reloadDuration
            : 0f;


    public bool FireCooldownElapsed =>
        Time.time >= nextFireTime;


    public bool CanFire =>
        weaponData != null &&
        CurrentAmmo > 0 &&
        FireCooldownElapsed;


    public bool CanReload =>
        weaponData != null &&
        CurrentAmmo < weaponData.maxAmmo;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        IdleState =
            new WeaponIdleState();

        FiringState =
            new WeaponFiringState();

        ReloadingState =
            new WeaponReloadingState();


        if (weaponData == null)
        {
            Debug.LogError(
                "[Weapon] WeaponData has not been assigned.",
                this
            );

            enabled = false;

            return;
        }


        if (inputReader == null)
        {
            Debug.LogError(
                "[Weapon] PlayerInputReader missing.",
                this
            );

            enabled = false;

            return;
        }


        if (aimController == null)
        {
            Debug.LogError(
                "[Weapon] AimController missing.",
                this
            );

            enabled = false;

            return;
        }


        if (muzzlePoint == null)
        {
            Debug.LogError(
                "[Weapon] MuzzlePoint missing.",
                this
            );

            enabled = false;

            return;
        }


        if (weaponData.projectilePrefab == null)
        {
            Debug.LogError(
                "[Weapon] Projectile prefab missing from WeaponData.",
                weaponData
            );

            enabled = false;

            return;
        }


        CurrentAmmo =
            weaponData.maxAmmo;
    }


    private void Start()
    {
        ChangeState(
            IdleState
        );


        AmmoChanged?.Invoke(
            CurrentAmmo,
            weaponData.maxAmmo
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        currentState?.Tick(
            this
        );
    }


    // =========================================================
    // STATE MACHINE
    // =========================================================

    public void ChangeState(
        IWeaponState nextState)
    {
        if (nextState == null ||
            ReferenceEquals(
                currentState,
                nextState))
        {
            return;
        }


        if (currentState != null &&
            !currentState.CanTransitionTo(nextState))
        {
            Debug.Log(
                $"[Weapon FSM] Blocked transition: " +
                $"{currentState.GetType().Name} → " +
                $"{nextState.GetType().Name}"
            );

            return;
        }


        currentState?.Exit(this);

        currentState =
            nextState;

        currentState.Enter(this);
    }


    // =========================================================
    // FIRE
    // =========================================================

    public void FireShot()
    {
        if (CurrentAmmo <= 0)
        {
            NotifyFireBlockedNoAmmo();

            return;
        }


        if (!FireCooldownElapsed)
            return;


        // Get aim information from the exact frame
        // in which the weapon fires.
        aimController.ResolveAim();


        Vector3 aimPoint =
            aimController.CurrentAimPoint;


        // -----------------------------------------------------
        // MUZZLE → CROSSHAIR AIM POINT
        // -----------------------------------------------------

        Vector3 shotDirection =
            aimPoint -
            muzzlePoint.position;


        if (shotDirection.sqrMagnitude <
            0.001f)
        {
            return;
        }


        shotDirection.Normalize();


        // -----------------------------------------------------
        // OPTIONAL SPREAD
        // -----------------------------------------------------

        shotDirection =
            ApplySpread(
                shotDirection
            );


        // -----------------------------------------------------
        // PROJECTILE ROTATION
        // -----------------------------------------------------

        Quaternion rotation =
            Quaternion.LookRotation(
                shotDirection,
                Vector3.up
            );


        // -----------------------------------------------------
        // SPAWN
        // -----------------------------------------------------

        GameObject projectileObject =
            Instantiate(
                weaponData.projectilePrefab,
                muzzlePoint.position,
                rotation
            );


        if (!projectileObject.TryGetComponent(
            out SimpleProjectile projectile))
        {
            Debug.LogError(
                "[Weapon] Projectile prefab does not contain SimpleProjectile.",
                projectileObject
            );


            Destroy(
                projectileObject
            );

            return;
        }


        // -----------------------------------------------------
        // INITIALIZE PROJECTILE
        // -----------------------------------------------------

        projectile.Initialize(
            shotDirection,
            weaponData.projectileSpeed,
            weaponData.damage,
            weaponData.maxRange,
            projectileHitMask
        );


        // =====================================================
        // COMMIT SHOT
        // =====================================================

        CurrentAmmo--;


        nextFireTime =
            Time.time +
            weaponData.fireInterval;


        AmmoChanged?.Invoke(
            CurrentAmmo,
            weaponData.maxAmmo
        );


        // Camera/audio/VFX systems listen to this.
        ShotFired?.Invoke();


        // -----------------------------------------------------
        // DEBUG
        // -----------------------------------------------------

        if (showShotDebug)
        {
            Debug.DrawRay(
                muzzlePoint.position,
                shotDirection *
                weaponData.maxRange,
                Color.red,
                1f
            );
        }

   
        Debug.Log(
            $"[Weapon] {weaponData.weaponName} fired. " +
            $"Ammo: {CurrentAmmo}/{weaponData.maxAmmo}"
        );
     
    }


    // =========================================================
    // SPREAD
    // =========================================================

    private Vector3 ApplySpread(
        Vector3 direction)
    {
        if (weaponData.spreadAngle <= 0f)
        {
            return direction;
        }


        float spread =
            weaponData.spreadAngle;


        Quaternion randomSpread =
            Quaternion.Euler(
                UnityEngine.Random.Range(
                    -spread,
                    spread
                ),
                UnityEngine.Random.Range(
                    -spread,
                    spread
                ),
                0f
            );


        return (
            randomSpread *
            direction
        ).normalized;
    }


    // =========================================================
    // RELOAD
    // =========================================================

    public void NotifyFireBlockedNoAmmo()
    {
        FireBlockedNoAmmo?.Invoke();


        Debug.Log(
            "[Weapon] Cannot fire — magazine empty."
        );
    }


    public void BeginReload()
    {
        ReloadStarted?.Invoke();
    }


    public void CompleteReload()
    {
        CurrentAmmo =
            weaponData.maxAmmo;


        AmmoChanged?.Invoke(
            CurrentAmmo,
            weaponData.maxAmmo
        );


        ReloadCompleted?.Invoke();


        Debug.Log(
            $"[Weapon] {weaponData.weaponName} reload complete."
        );
    }
}
