using System;
using UnityEngine;

public class CameraRigController : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]
    [Tooltip("PlayerRoot — the object this script rotates on Y.")]
    [SerializeField] private Transform root;

    [Tooltip("CameraRoot — child of PlayerRoot, owns yaw independently.")]
    [SerializeField] private Transform cameraYawPivot;

    [Tooltip("The child under CameraRoot that receives pitch (mouse-Y).")]
    [SerializeField] private Transform cameraPitchPivot;

    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerController playerController;

    // =========================================================
    // LOOK SETTINGS
    // =========================================================

    [Header("Look")]
    [SerializeField] private float yawSensitivity = 2f;
    [SerializeField] private float aimYawSensitivity = 1.2f;
    [SerializeField] private float pitchSensitivity = 2f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Cover")]
    [SerializeField] private CoverController coverController;

    [Header("Aim Pitch")]
    [SerializeField] private float aimPitchSensitivity = 1.2f;  // typically lower, more precise
    [SerializeField] private float aimMinPitch = -40f;
    [SerializeField] private float aimMaxPitch = 70f;

    // =========================================================
    // ROOT ROTATION SETTINGS
    // =========================================================

    [Header("Free Roam (not aiming)")]
    [SerializeField] private float freeRoamTurnSharpness = 8f;

    [Header("Aim / ADS")]
    [SerializeField] private float aimTurnSharpness = 20f;
    [SerializeField] private bool instantSnapWhileAiming = false;

    [Header("Turn In Place")]
    [SerializeField, Range(0f, 180f)] private float turnInPlaceTriggerAngle = 65f;
    [SerializeField, Min(0.01f)] private float turnInPlaceDuration = 0.55f;
    [SerializeField, Min(0f)] private float stationaryMovementThreshold = 0.05f;

    // =========================================================
    // STATE
    // =========================================================

    public bool IsAiming { get; private set; }

    public event Action<TurnInPlaceDirection> TurnInPlaceStarted;

    private float cameraYaw;
    private float cameraPitch;

    private bool isTurningInPlace;
    private float turnElapsed;
    private Quaternion turnStartRotation;
    private Quaternion turnTargetRotation;

    // =========================================================
    // INIT
    // =========================================================

    private void Start()
    {
        // Seed yaw from whatever the rig's starting rotation is,
        // so it doesn't snap on first frame.
        cameraYaw = root.eulerAngles.y;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void LateUpdate()
    {
        IsAiming = inputReader.IsAimHeld;
        ReadLookInput();
        ApplyRootRotation();
        ApplyCameraRotation();
    }

    // =========================================================
    // LOOK INPUT
    // =========================================================

    private void ReadLookInput()
    {
        Vector2 look = inputReader.LookInput;
        float currentPitchSens = IsAiming ? aimPitchSensitivity : pitchSensitivity;
        float currentMinPitch = IsAiming ? aimMinPitch : minPitch;
        float currentMaxPitch = IsAiming ? aimMaxPitch : maxPitch;

        cameraYaw += look.x * (IsAiming ? aimYawSensitivity : yawSensitivity);
        cameraPitch -= look.y * currentPitchSens;
        cameraPitch = Mathf.Clamp(cameraPitch, currentMinPitch, currentMaxPitch);
        //Debug.Log($"Aiming: {IsAiming}, MoveInput: {playerController.HasMovementInput}");
    }

    // =========================================================
    // CAMERA ROTATION (yaw pivot + pitch pivot)
    // =========================================================

    private void ApplyCameraRotation()
    {
        // Yaw pivot rotation is set directly from the accumulated value —
        // deliberately NOT reading root.rotation, so this stays fully
        // decoupled from wherever PlayerRoot currently faces.
        cameraYawPivot.rotation = Quaternion.Euler(0f, cameraYaw, 0f);

        // Pitch is local to the yaw pivot, so it tilts correctly
        // regardless of current yaw.
        cameraPitchPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    // =========================================================
    // ROOT ROTATION (Player facing)
    // =========================================================

    private void ApplyRootRotation()
    {
        if (coverController != null && coverController.IsInCover)
        {
            CancelTurnInPlace();
            return;
        }

        bool stationary = IsPlayerStationary();

        if (isTurningInPlace)
        {
            if (!stationary)
            {
                CancelTurnInPlace();
            }
            else
            {
                TickTurnInPlace();
                return;
            }
        }

        if (IsAiming && stationary)
        {
            TryStartTurnInPlace();
            return;
        }

        RotateRootForCurrentMode();
    }

    private bool IsPlayerStationary()
    {
        if (playerController == null)
            return true;

        Vector3 movement = playerController.MovementDirection;
        movement.y = 0f;

        return movement.sqrMagnitude <=
               stationaryMovementThreshold * stationaryMovementThreshold;
    }

    private float GetAimYawDelta()
    {
        return Mathf.DeltaAngle(root.eulerAngles.y, cameraYaw);
    }

    private void TryStartTurnInPlace()
    {
        float yawDelta = GetAimYawDelta();

        if (Mathf.Abs(yawDelta) < turnInPlaceTriggerAngle)
            return;

        float turnAmount = Mathf.Clamp(yawDelta, -90f, 90f);

        turnStartRotation = root.rotation;
        turnTargetRotation = Quaternion.Euler(
            0f,
            root.eulerAngles.y + turnAmount,
            0f
        );
        turnElapsed = 0f;
        isTurningInPlace = true;

        TurnInPlaceDirection direction =
            turnAmount > 0f
                ? TurnInPlaceDirection.Right
                : TurnInPlaceDirection.Left;

        TurnInPlaceStarted?.Invoke(direction);
    }

    private void TickTurnInPlace()
    {
        turnElapsed += Time.deltaTime;

        float normalizedTime = Mathf.Clamp01(
            turnElapsed / Mathf.Max(turnInPlaceDuration, 0.01f)
        );
        float smoothTime = Mathf.SmoothStep(0f, 1f, normalizedTime);

        root.rotation = Quaternion.Slerp(
            turnStartRotation,
            turnTargetRotation,
            smoothTime
        );

        if (normalizedTime < 1f)
            return;

        root.rotation = turnTargetRotation;
        isTurningInPlace = false;
    }

    private void CancelTurnInPlace()
    {
        isTurningInPlace = false;
        turnElapsed = 0f;
    }

    private void RotateRootForCurrentMode()
    {
        if (coverController != null && coverController.IsInCover)
            return;
        Quaternion targetYaw = Quaternion.Euler(0f, cameraYaw, 0f);

        if (IsAiming)
        {
            if (instantSnapWhileAiming)
            {
                root.rotation = targetYaw;
            }
            else
            {
                root.rotation = Quaternion.Slerp(
                    root.rotation,
                    targetYaw,
                    1f - Mathf.Exp(-aimTurnSharpness * Time.deltaTime)
                );
            }
        }
        else
        {
            Vector3 movementDirection = playerController.MovementDirection;

            movementDirection.y = 0f;

            if (movementDirection.sqrMagnitude < 0.001f)
                return;

            movementDirection.Normalize();

            targetYaw = Quaternion.LookRotation(movementDirection, Vector3.up);

            root.rotation = Quaternion.Slerp(
                root.rotation,
                targetYaw,
                1f - Mathf.Exp(-freeRoamTurnSharpness * Time.deltaTime)
            );
        }
        // Standing still, not aiming → Root untouched. Free orbit.
    }
}
