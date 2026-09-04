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
    }


    private void OnEnable()
    {
        if (playerController != null)
        {
            playerController.JumpStarted +=
                HandleJumpStarted;
        }
    }


    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.JumpStarted -=
                HandleJumpStarted;
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
    // EVENTS
    // =========================================================

    private void HandleJumpStarted()
    {
        animator.SetTrigger(
            JumpHash
        );
    }
}
