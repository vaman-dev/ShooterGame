//using UnityEngine;
//using Unity.Cinemachine;

//public class CameraModeSwitcher : MonoBehaviour
//{
//    [Header("Cameras")]
//    [SerializeField] private CinemachineCamera hipFireCamera;   // TPS_FollowCam
//    [SerializeField] private CinemachineCamera aimCamera;       // AimCam

//    [Header("Priorities")]
//    [SerializeField] private int activePriority = 20;
//    [SerializeField] private int inactivePriority = 10;

//    [Header("Input Source")]
//    [SerializeField] private CameraRigController cameraRig;   // reads IsAiming from here

//    private void Start()
//    {
//        // Ensure hip-fire is active on scene load
//        hipFireCamera.Priority = activePriority;
//        aimCamera.Priority = inactivePriority;
//    }

//    private void Update()
//    {
//        bool aiming = cameraRig.IsAiming;

//        hipFireCamera.Priority = aiming ? inactivePriority : activePriority;
//        aimCamera.Priority = aiming ? activePriority : inactivePriority;
//    }
//}


using UnityEngine;
using Unity.Cinemachine;

public class CameraModeSwitcher : MonoBehaviour
{
    // =========================================================
    // CAMERA MODE
    // =========================================================

    public enum CameraMode
    {
        HipFire,
        Aim,
        Sprint,
        Cover
    }


    // =========================================================
    // CAMERAS
    // =========================================================

    [Header("Cinemachine Cameras")]

    [Tooltip("Normal free-roam TPS camera.")]
    [SerializeField]
    private CinemachineCamera hipFireCamera;

    [Tooltip("Wider presentation camera used while the player is sprinting.")]
    [SerializeField]
    private CinemachineCamera sprintCamera;

    [Tooltip("Normal shoulder / ADS camera.")]
    [SerializeField]
    private CinemachineCamera aimCamera;

    [Tooltip("Camera used while the player is attached to cover.")]
    [SerializeField]
    private CinemachineCamera coverCamera;


    // =========================================================
    // PRIORITIES
    // =========================================================

    [Header("Priorities")]

    [SerializeField]
    private int activePriority = 20;

    [SerializeField]
    private int inactivePriority = 10;


    // =========================================================
    // GAMEPLAY REFERENCES
    // =========================================================

    [Header("Gameplay References")]

    [Tooltip("Provides normal Aim/ADS state.")]
    [SerializeField]
    private CameraRigController cameraRig;

    [Tooltip("Provides cover state.")]
    [SerializeField]
    private CoverController coverController;

    [Tooltip("Provides sprint state from the gameplay state machine.")]
    [SerializeField]
    private PlayerController playerController;


    // =========================================================
    // STATE
    // =========================================================

    public CameraMode CurrentMode
    {
        get;
        private set;
    }


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Start()
    {
        SetCameraMode(
            DetermineCameraMode(),
            true
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        CameraMode desiredMode =
            DetermineCameraMode();


        if (desiredMode == CurrentMode)
            return;


        SetCameraMode(
            desiredMode
        );
    }


    // =========================================================
    // MODE DECISION
    // =========================================================

    private CameraMode DetermineCameraMode()
    {
        // -----------------------------------------------------
        // COVER HAS HIGHEST PRIORITY
        // -----------------------------------------------------

        if (coverController != null &&
            coverController.IsInCover)
        {
            return CameraMode.Cover;
        }


        // -----------------------------------------------------
        // NORMAL AIM / ADS
        // -----------------------------------------------------

        if (cameraRig != null &&
            cameraRig.IsAiming)
        {
            return CameraMode.Aim;
        }


        // -----------------------------------------------------
        // SPRINT PRESENTATION
        // -----------------------------------------------------

        if (playerController != null &&
            playerController.IsSprinting)
        {
            return CameraMode.Sprint;
        }


        // -----------------------------------------------------
        // DEFAULT
        // -----------------------------------------------------

        return CameraMode.HipFire;
    }


    // =========================================================
    // CAMERA SWITCHING
    // =========================================================

    public void SetCameraMode(
        CameraMode newMode,
        bool force = false)
    {
        if (!force &&
            newMode == CurrentMode)
        {
            return;
        }


        CurrentMode =
            newMode;


        SetCameraPriority(
            hipFireCamera,
            newMode == CameraMode.HipFire
        );


        SetCameraPriority(
            sprintCamera,
            newMode == CameraMode.Sprint
        );


        SetCameraPriority(
            aimCamera,
            newMode == CameraMode.Aim
        );


        SetCameraPriority(
            coverCamera,
            newMode == CameraMode.Cover
        );
    }


    // =========================================================
    // PRIORITY HELPER
    // =========================================================

    private void SetCameraPriority(
        CinemachineCamera camera,
        bool active)
    {
        if (camera == null)
            return;


        camera.Priority =
            active
                ? activePriority
                : inactivePriority;
    }
}
