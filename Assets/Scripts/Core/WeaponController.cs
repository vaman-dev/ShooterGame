using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private AimController aimController;

    [Header("Weapon Data")]
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private float fireRate = 0.15f; // seconds between shots
    [SerializeField] private float reloadDuration = 1.5f;

    private float fireCooldownTimer;
    private IWeaponState currentState;

    public WeaponIdleState IdleState { get; private set; }
    public WeaponFiringState FiringState { get; private set; }
    public WeaponReloadingState ReloadingState { get; private set; }

    public PlayerInputReader InputReader => inputReader;
    public int CurrentAmmo { get; private set; }
    public float ReloadDuration => reloadDuration;

    public bool CanFire => CurrentAmmo > 0 && FireCooldownElapsed;
    public bool CanReload => CurrentAmmo < maxAmmo;
    public bool FireCooldownElapsed => fireCooldownTimer <= 0f;

    private void Awake()
    {
        IdleState = new WeaponIdleState();
        FiringState = new WeaponFiringState();
        ReloadingState = new WeaponReloadingState();

        CurrentAmmo = maxAmmo;
    }

    private void Start()
    {
        ChangeState(IdleState);
    }

    private void Update()
    {
        if (fireCooldownTimer > 0f)
            fireCooldownTimer -= Time.deltaTime;

        currentState?.Tick(this);
    }

    public void ChangeState(IWeaponState nextState)
    {
        if (nextState == null || ReferenceEquals(currentState, nextState))
            return;

        if (currentState != null && !currentState.CanTransitionTo(nextState))
        {
            Debug.Log($"[Weapon FSM] Blocked transition: {currentState.GetType().Name} → {nextState.GetType().Name}");
            return;
        }

        currentState?.Exit(this);
        currentState = nextState;
        currentState.Enter(this);
    }

    public void FireShot()
    {
        CurrentAmmo--;
        fireCooldownTimer = fireRate;

        Vector3 aimPoint = aimController.CurrentAimPoint;
        Vector3 aimDirection = aimController.CurrentAimDirection;

        Debug.Log($"[Weapon] Fired shot toward {aimPoint} — Ammo remaining: {CurrentAmmo}");

        // Actual raycast/hit-detection or projectile spawn goes here next.
    }

    public void CompleteReload()
    {
        CurrentAmmo = maxAmmo;
        Debug.Log("[Weapon] Reload complete.");
    }
}