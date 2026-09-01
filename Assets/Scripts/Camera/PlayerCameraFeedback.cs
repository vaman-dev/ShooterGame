using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraFeedback : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]
    [SerializeField] private PlayerController playerController;


    // =========================================================
    // IMPULSE SOURCES
    // =========================================================

    [Header("Impulse Sources")]

    [Tooltip("Impulse Source configured on the PlayerMovement channel.")]
    [SerializeField] private CinemachineImpulseSource jumpImpulseSource;

    [Tooltip("Impulse Source configured on the PlayerMovement channel.")]
    [SerializeField] private CinemachineImpulseSource landingImpulseSource;

    [Tooltip("Impulse Source configured on the Weapon channel.")]
    [SerializeField] private CinemachineImpulseSource weaponImpulseSource;

    [Tooltip("Impulse Source configured on the Damage channel.")]
    [SerializeField] private CinemachineImpulseSource damageImpulseSource;

    [Tooltip("Impulse Source configured on the Explosion channel.")]
    [SerializeField] private CinemachineImpulseSource explosionImpulseSource;

    [Tooltip("Impulse Source configured on the Environment channel.")]
    [SerializeField] private CinemachineImpulseSource environmentImpulseSource;


    // =========================================================
    // JUMP
    // =========================================================

    [Header("Jump")]

    [SerializeField]
    private float jumpImpulseStrength = 0.3f;

    [SerializeField]
    private Vector3 jumpImpulseDirection = Vector3.down;


    // =========================================================
    // LANDING
    // =========================================================

    [Header("Landing")]

    [Tooltip("Minimum downward speed required before landing shake begins.")]
    [SerializeField]
    private float minimumLandingSpeed = 2f;

    [Tooltip("Landing speed considered a maximum-strength impact.")]
    [SerializeField]
    private float maximumLandingSpeed = 10f;

    [SerializeField]
    private float minimumLandingImpulseStrength = 0.35f;

    [SerializeField]
    private float maximumLandingImpulseStrength = 0.85f;

    [SerializeField]
    private Vector3 landingImpulseDirection = Vector3.down;


    // =========================================================
    // WEAPON
    // =========================================================

    [Header("Weapon")]

    [SerializeField]
    private float defaultWeaponImpulseStrength = 0.2f;

    [SerializeField]
    private Vector3 weaponImpulseDirection = Vector3.back;


    // =========================================================
    // DAMAGE
    // =========================================================

    [Header("Damage")]

    [SerializeField]
    private float defaultDamageImpulseStrength = 0.5f;

    [SerializeField]
    private Vector3 damageImpulseDirection = Vector3.back;


    // =========================================================
    // EXPLOSION
    // =========================================================

    [Header("Explosion")]

    [SerializeField]
    private float defaultExplosionImpulseStrength = 1f;

    [SerializeField]
    private Vector3 explosionImpulseDirection = Vector3.up;


    // =========================================================
    // ENVIRONMENT
    // =========================================================

    [Header("Environment")]

    [SerializeField]
    private float defaultEnvironmentImpulseStrength = 0.5f;

    [SerializeField]
    private Vector3 environmentImpulseDirection = Vector3.down;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        if (playerController == null)
        {
            playerController =
                GetComponent<PlayerController>();
        }
    }


    // =========================================================
    // EVENT REGISTRATION
    // =========================================================

    private void OnEnable()
    {
        if (playerController == null)
            return;


        playerController.JumpStarted +=
            HandleJumpStarted;


        playerController.Landed +=
            HandleLanded;
    }


    private void OnDisable()
    {
        if (playerController == null)
            return;


        playerController.JumpStarted -=
            HandleJumpStarted;


        playerController.Landed -=
            HandleLanded;
    }


    // =========================================================
    // PLAYER MOVEMENT EVENTS
    // =========================================================

    private void HandleJumpStarted()
    {
        PlayJumpImpulse();
    }


    private void HandleLanded(float downwardSpeed)
    {
        PlayLandingImpulse(
            downwardSpeed
        );
    }


    // =========================================================
    // JUMP
    // =========================================================

    public void PlayJumpImpulse()
    {
        GenerateDirectionalImpulse(
            jumpImpulseSource,
            jumpImpulseDirection,
            jumpImpulseStrength
        );
    }


    // =========================================================
    // LANDING
    // =========================================================

    public void PlayLandingImpulse(float downwardSpeed)
    {
        if (landingImpulseSource == null)
            return;


        if (downwardSpeed < minimumLandingSpeed)
            return;


        float normalizedImpact =
            Mathf.InverseLerp(
                minimumLandingSpeed,
                maximumLandingSpeed,
                downwardSpeed
            );


        float strength =
            Mathf.Lerp(
                minimumLandingImpulseStrength,
                maximumLandingImpulseStrength,
                normalizedImpact
            );


        GenerateDirectionalImpulse(
            landingImpulseSource,
            landingImpulseDirection,
            strength
        );
    }


    // =========================================================
    // WEAPON
    // =========================================================

    public void PlayWeaponImpulse()
    {
        PlayWeaponImpulse(
            defaultWeaponImpulseStrength
        );
    }


    public void PlayWeaponImpulse(float strength)
    {
        GenerateDirectionalImpulse(
            weaponImpulseSource,
            weaponImpulseDirection,
            strength
        );
    }


    public void PlayWeaponImpulse(
        Vector3 direction,
        float strength)
    {
        GenerateDirectionalImpulse(
            weaponImpulseSource,
            direction,
            strength
        );
    }


    // =========================================================
    // DAMAGE
    // =========================================================

    public void PlayDamageImpulse()
    {
        PlayDamageImpulse(
            defaultDamageImpulseStrength
        );
    }


    public void PlayDamageImpulse(float strength)
    {
        GenerateDirectionalImpulse(
            damageImpulseSource,
            damageImpulseDirection,
            strength
        );
    }


    public void PlayDamageImpulse(
        Vector3 direction,
        float strength)
    {
        GenerateDirectionalImpulse(
            damageImpulseSource,
            direction,
            strength
        );
    }


    // =========================================================
    // EXPLOSION
    // =========================================================

    public void PlayExplosionImpulse()
    {
        PlayExplosionImpulse(
            defaultExplosionImpulseStrength
        );
    }


    public void PlayExplosionImpulse(float strength)
    {
        GenerateDirectionalImpulse(
            explosionImpulseSource,
            explosionImpulseDirection,
            strength
        );
    }


    public void PlayExplosionImpulse(
        Vector3 direction,
        float strength)
    {
        GenerateDirectionalImpulse(
            explosionImpulseSource,
            direction,
            strength
        );
    }


    // =========================================================
    // ENVIRONMENT
    // =========================================================

    public void PlayEnvironmentImpulse()
    {
        PlayEnvironmentImpulse(
            defaultEnvironmentImpulseStrength
        );
    }


    public void PlayEnvironmentImpulse(float strength)
    {
        GenerateDirectionalImpulse(
            environmentImpulseSource,
            environmentImpulseDirection,
            strength
        );
    }


    public void PlayEnvironmentImpulse(
        Vector3 direction,
        float strength)
    {
        GenerateDirectionalImpulse(
            environmentImpulseSource,
            direction,
            strength
        );
    }


    // =========================================================
    // UNIVERSAL IMPULSE METHOD
    // =========================================================

    private void GenerateDirectionalImpulse(
        CinemachineImpulseSource source,
        Vector3 direction,
        float strength)
    {
        if (source == null)
            return;


        if (strength <= 0f)
            return;


        Vector3 normalizedDirection =
            direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector3.down;


        source.GenerateImpulse(
            normalizedDirection * strength
        );
    }
}