using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private CameraRigController cameraRigController;

    [SerializeField]
    private WeaponAttachmentController weaponAttachmentController;

    [SerializeField]
    private WeaponController weaponController;


    // =========================================================
    // RIFLE LAYER BLEND
    // =========================================================

    [Header("Rifle Layer Blend")]

    [SerializeField]
    private string rifleLayerName = "Rifle Layer";

    [SerializeField]
    [Min(0.01f)]
    private float rifleLayerBlendDuration = 0.2f;


    // =========================================================
    // LOCOMOTION SETTINGS
    // =========================================================

    [Header("Locomotion")]

    [SerializeField]
    [Min(0f)]
    private float directionDampTime = 0.1f;

    [SerializeField]
    [Min(0f)]
    private float movementDampTime = 0.1f;


    // =========================================================
    // ANIMATOR HASHES
    // =========================================================

    private static readonly int MoveXHash =
        Animator.StringToHash("MoveX");

    private static readonly int MoveYHash =
        Animator.StringToHash("MoveY");

    private static readonly int MoveAmountHash =
        Animator.StringToHash("MoveAmount");

    private static readonly int IsSprintingHash =
        Animator.StringToHash("IsSprinting");

    private static readonly int IsGroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int VerticalVelocityHash =
        Animator.StringToHash("VerticalVelocity");

    private static readonly int JumpHash =
        Animator.StringToHash("Jump");

    private static readonly int TurnLeft90Hash =
        Animator.StringToHash("TurnLeft90");

    private static readonly int TurnRight90Hash =
        Animator.StringToHash("TurnRight90");

    private static readonly int HasRifleHash =
        Animator.StringToHash("HasRifle");

    private static readonly int IsAimingHash =
        Animator.StringToHash("IsAiming");

    private static readonly int FireHash =
        Animator.StringToHash("Fire");

    private static readonly int ReloadHash =
        Animator.StringToHash("Reload");

    private int rifleLayerIndex = -1;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>(true);
        }


        if (playerController == null)
        {
            playerController =
                GetComponentInParent<PlayerController>();
        }


        if (cameraRigController == null &&
            playerController != null)
        {
            cameraRigController =
                playerController.GetComponentInChildren<CameraRigController>(true);
        }

        if (weaponAttachmentController == null &&
            playerController != null)
        {
            weaponAttachmentController =
                playerController.GetComponent<WeaponAttachmentController>();
        }

        if (weaponController == null &&
            playerController != null)
        {
            weaponController =
                playerController.GetComponent<WeaponController>();
        }

        if (animator != null)
        {
            rifleLayerIndex =
                animator.GetLayerIndex(rifleLayerName);
        }
    }


    private void OnEnable()
    {
        if (playerController != null)
        {
            playerController.JumpStarted +=
                HandleJumpStarted;
        }


        if (cameraRigController != null)
        {
            cameraRigController.TurnInPlaceStarted +=
                HandleTurnInPlaceStarted;
        }

        if (weaponController != null)
        {
            weaponController.ShotFired +=
                HandleShotFired;

            weaponController.ReloadStarted +=
                HandleReloadStarted;
        }
    }


    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.JumpStarted -=
                HandleJumpStarted;
        }


        if (cameraRigController != null)
        {
            cameraRigController.TurnInPlaceStarted -=
                HandleTurnInPlaceStarted;
        }

        if (weaponController != null)
        {
            weaponController.ShotFired -=
                HandleShotFired;

            weaponController.ReloadStarted -=
                HandleReloadStarted;
        }
    }


    private void Update()
    {
        if (animator == null ||
            playerController == null)
        {
            return;
        }


        UpdateLocomotion();
        UpdateAirState();
        UpdateWeaponState();
    }


    // =========================================================
    // LOCOMOTION
    // =========================================================

    private void UpdateLocomotion()
    {
        Vector3 worldMovement =
            playerController.MovementDirection;


        worldMovement.y =
            0f;


        float moveAmount =
            Mathf.Clamp01(
                worldMovement.magnitude
            );


        float movementScale =
            moveAmount > 0.001f
                ? playerController.IsSprinting
                    ? 1f
                    : 0.5f
                : 0f;


        Vector3 localMovement =
            Vector3.zero;


        if (worldMovement.sqrMagnitude > 0.001f)
        {
            localMovement =
                playerController.transform.InverseTransformDirection(
                    worldMovement.normalized
                );
        }


        animator.SetFloat(
            MoveXHash,
            localMovement.x * movementScale,
            directionDampTime,
            Time.deltaTime
        );


        animator.SetFloat(
            MoveYHash,
            localMovement.z * movementScale,
            directionDampTime,
            Time.deltaTime
        );


        animator.SetFloat(
            MoveAmountHash,
            moveAmount,
            movementDampTime,
            Time.deltaTime
        );


        animator.SetBool(
            IsSprintingHash,
            playerController.IsSprinting
        );
    }


    // =========================================================
    // AIR STATE
    // =========================================================

    private void UpdateAirState()
    {
        animator.SetBool(
            IsGroundedHash,
            playerController.IsGrounded
        );


        animator.SetFloat(
            VerticalVelocityHash,
            playerController.VerticalVelocity
        );
    }


    // =========================================================
    // WEAPON ANIMATION LAYER
    // =========================================================

    private void UpdateWeaponState()
    {
        bool hasRifle =
            weaponAttachmentController != null &&
            weaponAttachmentController.IsEquipped;

        bool isAiming =
            cameraRigController != null &&
            cameraRigController.IsAiming;

        animator.SetBool(
            HasRifleHash,
            hasRifle
        );

        animator.SetBool(
            IsAimingHash,
            hasRifle && isAiming
        );

        BlendRifleLayer(
            hasRifle
        );
    }


    private void BlendRifleLayer(bool hasRifle)
    {
        if (rifleLayerIndex < 0 ||
            rifleLayerIndex >= animator.layerCount)
        {
            return;
        }

        float targetWeight =
            hasRifle
                ? 1f
                : 0f;

        float blendDuration =
            Mathf.Max(
                0.01f,
                rifleLayerBlendDuration
            );

        float currentWeight =
            animator.GetLayerWeight(
                rifleLayerIndex
            );

        float nextWeight =
            Mathf.MoveTowards(
                currentWeight,
                targetWeight,
                Time.deltaTime / blendDuration
            );

        if (!Mathf.Approximately(currentWeight, nextWeight))
        {
            animator.SetLayerWeight(
                rifleLayerIndex,
                nextWeight
            );
        }
    }


    // =========================================================
    // EVENTS
    // =========================================================

    private void HandleJumpStarted()
    {
        animator.SetTrigger(
            JumpHash
        );
    }


    private void HandleTurnInPlaceStarted(
        TurnInPlaceDirection direction)
    {
        if (animator == null)
            return;

        switch (direction)
        {
            case TurnInPlaceDirection.Left:
                animator.SetTrigger(TurnLeft90Hash);
                break;

            case TurnInPlaceDirection.Right:
                animator.SetTrigger(TurnRight90Hash);
                break;
        }
    }


    private void HandleShotFired()
    {
        if (animator == null)
            return;

        animator.SetTrigger(FireHash);
    }


    private void HandleReloadStarted()
    {
        if (animator == null)
            return;

        animator.SetTrigger(ReloadHash);
    }
}
