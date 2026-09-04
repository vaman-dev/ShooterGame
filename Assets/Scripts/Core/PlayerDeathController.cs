using UnityEngine;

public class PlayerDeathController : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private WeaponFirePolicy weaponFirePolicy;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private CoverController coverController;

    [SerializeField]
    private CoverPeekController coverPeekController;


    // =========================================================
    // STATE
    // =========================================================

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
        if (playerHealth == null)
        {
            playerHealth =
                GetComponent<PlayerHealth>();
        }


        if (weaponFirePolicy == null)
        {
            weaponFirePolicy =
                GetComponent<WeaponFirePolicy>();
        }


        if (playerController == null)
        {
            playerController =
                GetComponent<PlayerController>();
        }


        if (coverController == null)
        {
            coverController =
                GetComponent<CoverController>();
        }


        if (coverPeekController == null)
        {
            coverPeekController =
                GetComponent<CoverPeekController>();
        }
    }


    // =========================================================
    // EVENTS
    // =========================================================

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died +=
                HandlePlayerDied;
        }
    }


    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -=
                HandlePlayerDied;
        }
    }


    // =========================================================
    // DEATH
    // =========================================================

    private void HandlePlayerDied()
    {
        if (IsDead)
            return;


        IsDead =
            true;


        // -----------------------------------------------------
        // Stop cover presentation/state first.
        // -----------------------------------------------------

        if (coverPeekController != null)
        {
            coverPeekController.ForceNeutral();
        }


        // -----------------------------------------------------
        // Disable combat authorization.
        // -----------------------------------------------------

        if (weaponFirePolicy != null)
        {
            weaponFirePolicy.SetCombatEnabled(
                false
            );
        }


        // -----------------------------------------------------
        // Disable gameplay movement.
        // -----------------------------------------------------

        if (playerController != null)
        {
            playerController.enabled =
                false;
        }


        if (coverController != null)
        {
            coverController.enabled =
                false;
        }


        if (coverPeekController != null)
        {
            coverPeekController.enabled =
                false;
        }


        Debug.Log(
            "[PlayerDeath] Player gameplay disabled.",
            this
        );
    }
}