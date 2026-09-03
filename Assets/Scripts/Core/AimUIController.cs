using UnityEngine;

public class AimUIController : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private CameraRigController cameraRigController;

    [SerializeField]
    private CoverPeekController coverPeekController;

    [SerializeField]
    private GameObject crosshair;


    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Settings")]

    [Tooltip(
        "If enabled, the crosshair is also shown " +
        "while peeking from cover."
    )]
    [SerializeField]
    private bool showWhileCoverPeeking = true;


    // =========================================================
    // RUNTIME
    // =========================================================

    private bool previousVisibleState;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Start()
    {
        UpdateAimUI(true);
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        UpdateAimUI(false);
    }


    // =========================================================
    // AIM UI
    // =========================================================

    private void UpdateAimUI(bool force)
    {
        bool normalAim =
            cameraRigController != null &&
            cameraRigController.IsAiming;


        bool coverAim =
            showWhileCoverPeeking &&
            coverPeekController != null &&
            coverPeekController.IsPeeking;


        bool shouldShow =
            normalAim ||
            coverAim;


        if (!force &&
            shouldShow == previousVisibleState)
        {
            return;
        }


        previousVisibleState =
            shouldShow;


        if (crosshair != null)
        {
            crosshair.SetActive(
                shouldShow
            );
        }
    }
}