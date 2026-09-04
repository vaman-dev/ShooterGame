using TMPro;
using UnityEngine;

public class WeaponReloadUI : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private WeaponController weaponController;

    [SerializeField]
    private GameObject reloadRoot;

    [SerializeField]
    private TMP_Text reloadText;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        if (reloadRoot == null)
        {
            reloadRoot =
                gameObject;
        }


        if (reloadText != null)
        {
            reloadText.text =
                "RELOADING...";
        }


        SetReloadVisible(false);
    }


    // =========================================================
    // EVENT REGISTRATION
    // =========================================================

    private void OnEnable()
    {
        if (weaponController == null)
            return;


        weaponController.ReloadStarted +=
            HandleReloadStarted;

        weaponController.ReloadCompleted +=
            HandleReloadCompleted;
    }


    private void OnDisable()
    {
        if (weaponController == null)
            return;


        weaponController.ReloadStarted -=
            HandleReloadStarted;

        weaponController.ReloadCompleted -=
            HandleReloadCompleted;
    }


    // =========================================================
    // EVENTS
    // =========================================================

    private void HandleReloadStarted()
    {
        SetReloadVisible(true);
    }


    private void HandleReloadCompleted()
    {
        SetReloadVisible(false);
    }


    // =========================================================
    // UI
    // =========================================================

    private void SetReloadVisible(bool visible)
    {
        if (reloadRoot == null)
            return;


        reloadRoot.SetActive(visible);
    }
}