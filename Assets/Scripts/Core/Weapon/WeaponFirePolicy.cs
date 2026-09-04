using UnityEngine;

public class WeaponFirePolicy : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private PlayerInputReader inputReader;

    [SerializeField]
    private CoverController coverController;

    [SerializeField]
    private CoverPeekController coverPeekController;


    // =========================================================
    // COMBAT STATE
    // =========================================================

    [Header("Combat State")]

    [SerializeField]
    private bool combatEnabled = true;


    // =========================================================
    // PUBLIC STATE
    // =========================================================

    public bool CombatEnabled =>
        combatEnabled;


    public bool CanAttemptFire
    {
        get
        {
            if (!combatEnabled)
                return false;


            if (inputReader == null ||
                coverController == null ||
                coverPeekController == null)
            {
                return false;
            }


            // The current vertical slice requires ADS/RMB in every
            // gameplay state where firing is permitted.
            if (!inputReader.IsAimHeld)
                return false;


            // Normal ADS outside cover is allowed.
            if (!coverController.IsInCover)
                return true;


            // Neutral cover is blocked. A valid left or right peek
            // is the only cover state from which firing is allowed.
            return coverPeekController.IsPeeking;
        }
    }


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        if (inputReader == null)
        {
            inputReader =
                GetComponent<PlayerInputReader>();
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


        ValidateReferences();
    }


    // =========================================================
    // COMBAT CONTROL
    // =========================================================

    public void SetCombatEnabled(bool isEnabled)
    {
        combatEnabled =
            isEnabled;
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private void ValidateReferences()
    {
        if (inputReader == null)
        {
            Debug.LogError(
                "[WeaponFirePolicy] PlayerInputReader is missing.",
                this
            );
        }


        if (coverController == null)
        {
            Debug.LogError(
                "[WeaponFirePolicy] CoverController is missing.",
                this
            );
        }


        if (coverPeekController == null)
        {
            Debug.LogError(
                "[WeaponFirePolicy] CoverPeekController is missing.",
                this
            );
        }
    }
}
