//using UnityEngine;

//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(PlayerInputReader))]
//public class PlayerController : MonoBehaviour
//{
//    // =========================================================
//    // REFERENCES
//    // =========================================================

//    [Header("References")]
//    [SerializeField] private CharacterController characterController;
//    [SerializeField] private PlayerInputReader inputReader;

//    [Tooltip("Assign the actual Main Camera here, not CameraRoot.")]
//    [SerializeField] private Transform cameraTransform;

//    // =========================================================
//    // GROUND MOVEMENT
//    // =========================================================

//    [Header("Ground Movement")]
//    [SerializeField] private float walkSpeed = 4f;
//    [SerializeField] private float sprintSpeed = 7f;

//    // =========================================================
//    // AIR MOVEMENT
//    // =========================================================

//    [Header("Jump / Gravity")]
//    [SerializeField] private float jumpForce = 7f;
//    [SerializeField] private float gravity = -20f;
//    [SerializeField] private float groundedGravity = -2f;
//    [SerializeField] private float airMovementSpeed = 4f;

//    // =========================================================
//    // RUNTIME
//    // =========================================================

//    private Vector3 velocity;
//    private IMovementState currentState;

//    // =========================================================
//    // STATES
//    // =========================================================

//    public NormalMovementState NormalState { get; private set; }
//    public SprintMovementState SprintState { get; private set; }
//    public JumpMovementState JumpState { get; private set; }

//    // =========================================================
//    // PUBLIC ACCESS
//    // =========================================================

//    public PlayerInputReader InputReader => inputReader;
//    public float WalkSpeed => walkSpeed;
//    public float SprintSpeed => sprintSpeed;
//    public bool IsGrounded => characterController.isGrounded;
//    public float VerticalVelocity => velocity.y;
//    public Vector3 Velocity => velocity;

//    public bool HasMovementInput =>
//        inputReader.MoveInput.sqrMagnitude > 0.01f;

//    // The same camera-relative direction used to move the CharacterController.
//    // CameraRigController uses this in free roam so the visible character faces
//    // where it is moving (for example, S faces camera-backward).
//    public Vector3 MovementDirection => GetCameraRelativeMovementDirection();

//    // =========================================================
//    // INITIALIZATION
//    // =========================================================

//    private void Awake()
//    {
//        if (characterController == null)
//            characterController = GetComponent<CharacterController>();

//        if (inputReader == null)
//            inputReader = GetComponent<PlayerInputReader>();

//        NormalState = new NormalMovementState();
//        SprintState = new SprintMovementState();
//        JumpState = new JumpMovementState();
//    }

//    private void Start()
//    {
//        ChangeState(NormalState);
//    }

//    // =========================================================
//    // UPDATE
//    // =========================================================

//    private void Update()
//    {
//        currentState?.Tick(this);
//    }

//    // =========================================================
//    // STATE MACHINE
//    // =========================================================

//    public void ChangeState(IMovementState nextState)
//    {
//        if (nextState == null)
//            return;

//        if (ReferenceEquals(currentState, nextState))
//            return;

//        if (currentState != null && !currentState.CanTransitionTo(nextState))
//            return;

//        currentState?.Exit(this);
//        currentState = nextState;
//        currentState.Enter(this);
//    }

//    // =========================================================
//    // GROUND MOVEMENT
//    // =========================================================

//    public void TickGroundMovement(float movementSpeed)
//    {
//        Vector3 movementDirection = GetCameraRelativeMovementDirection();

//        // NOTE: PlayerController never rotates PlayerRoot.
//        // Rotation ownership belongs entirely to CameraRigController.

//        velocity.x = movementDirection.x * movementSpeed;
//        velocity.z = movementDirection.z * movementSpeed;

//        if (IsGrounded && velocity.y < 0f)
//            velocity.y = groundedGravity;
//        else
//            ApplyGravity();

//        MoveCharacter();
//    }

//    // =========================================================
//    // JUMP
//    // =========================================================

//    public void BeginJump()
//    {
//        velocity.y = jumpForce;
//    }

//    public void TickAirMovement()
//    {
//        Vector3 movementDirection = GetCameraRelativeMovementDirection();

//        velocity.x = movementDirection.x * airMovementSpeed;
//        velocity.z = movementDirection.z * airMovementSpeed;

//        ApplyGravity();
//        MoveCharacter();
//    }

//    // =========================================================
//    // CHARACTER MOVEMENT
//    // =========================================================

//    private void MoveCharacter()
//    {
//        characterController.Move(velocity * Time.deltaTime);
//    }

//    // =========================================================
//    // GRAVITY
//    // =========================================================

//    private void ApplyGravity()
//    {
//        velocity.y += gravity * Time.deltaTime;
//    }

//    public void SnapToGround()
//    {
//        if (IsGrounded && velocity.y < 0f)
//            velocity.y = groundedGravity;
//    }

//    // =========================================================
//    // CAMERA-RELATIVE MOVEMENT
//    // =========================================================

//    private Vector3 GetCameraRelativeMovementDirection()
//    {
//        Vector2 input = inputReader.MoveInput;

//        if (input.sqrMagnitude <= 0.01f)
//            return Vector3.zero;

//        Vector3 cameraForward = cameraTransform.forward;
//        Vector3 cameraRight = cameraTransform.right;

//        cameraForward.y = 0f;
//        cameraRight.y = 0f;

//        cameraForward.Normalize();
//        cameraRight.Normalize();

//        Vector3 direction = cameraForward * input.y + cameraRight * input.x;

//        return Vector3.ClampMagnitude(direction, 1f);
//    }
//}


using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerController : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInputReader inputReader;

    [Tooltip("Assign the actual Main Camera here, not CameraRoot.")]
    [SerializeField] private Transform cameraTransform;


    // =========================================================
    // GROUND MOVEMENT
    // =========================================================

    [Header("Ground Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;


    // =========================================================
    // AIR MOVEMENT
    // =========================================================

    [Header("Jump / Gravity")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;
    [SerializeField] private float airMovementSpeed = 4f;


    // =========================================================
    // Cover
    // =========================================================
    [SerializeField] private CoverController coverController;

    // =========================================================
    // RUNTIME
    // =========================================================

    private Vector3 velocity;

    // IMPORTANT:
    // Cached world-space direction actually used for movement.
    //
    // CameraRigController reads THIS during Free Roam.
    private Vector3 movementDirection;

    private IMovementState currentState;


    // =========================================================
    // GAMEPLAY EVENTS
    // =========================================================

    /// <summary>
    /// Fired once when a valid jump actually begins.
    ///
    /// Camera feedback, audio, VFX, etc. can subscribe
    /// without PlayerController knowing about those systems.
    /// </summary>
    public event Action JumpStarted;


    /// <summary>
    /// Fired when the player returns to the ground after a jump.
    /// The argument is the downward speed at the instant of landing.
    /// </summary>
    public event Action<float> Landed;


    // =========================================================
    // STATES
    // =========================================================

    public NormalMovementState NormalState { get; private set; }

    public SprintMovementState SprintState { get; private set; }

    public JumpMovementState JumpState { get; private set; }


    // =========================================================
    // PUBLIC ACCESS
    // =========================================================

    public PlayerInputReader InputReader => inputReader;

    public float WalkSpeed => walkSpeed;

    public float SprintSpeed => sprintSpeed;

    public bool IsGrounded =>
        characterController.isGrounded;

    public float VerticalVelocity =>
        velocity.y;

    public Vector3 Velocity =>
        velocity;


    // =========================================================
    // MOVEMENT INFORMATION
    // =========================================================

    public bool HasMovementInput =>
        inputReader.MoveInput.sqrMagnitude > 0.01f;


    /// <summary>
    /// Camera-relative world-space movement direction
    /// used by the CharacterController this frame.
    ///
    /// CameraRigController uses this during Free Roam
    /// to orient PlayerRoot toward actual movement.
    /// </summary>
    public Vector3 MovementDirection =>
        movementDirection;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        if (characterController == null)
        {
            characterController =
                GetComponent<CharacterController>();
        }


        if (inputReader == null)
        {
            inputReader =
                GetComponent<PlayerInputReader>();
        }


        // States are created ONCE and reused.
        NormalState =
            new NormalMovementState();

        SprintState =
            new SprintMovementState();

        JumpState =
            new JumpMovementState();
    }


    private void Start()
    {
        ChangeState(
            NormalState
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (coverController != null && coverController.IsInCover)
            return; // Skip normal movement/state ticking entirely while in cover

        currentState?.Tick(this);
    }


    // =========================================================
    // STATE MACHINE
    // =========================================================

    public void ChangeState(IMovementState nextState)
    {
        if (nextState == null)
            return;


        if (ReferenceEquals(
            currentState,
            nextState))
        {
            return;
        }


        if (currentState != null &&
            !currentState.CanTransitionTo(nextState))
        {
            return;
        }


        currentState?.Exit(this);


        currentState =
            nextState;


        currentState.Enter(this);
    }


    // =========================================================
    // GROUND MOVEMENT
    // =========================================================

    public void TickGroundMovement(
        float movementSpeed)
    {
        // Calculate ONCE.
        //
        // The exact same direction is then:
        //
        // 1. Used for movement.
        // 2. Exposed to CameraRigController.
        //
        // This avoids two systems recalculating independently.

        movementDirection =
            GetCameraRelativeMovementDirection();


        velocity.x =
            movementDirection.x *
            movementSpeed;

        velocity.z =
            movementDirection.z *
            movementSpeed;


        // ---------------------------------------------
        // Ground / Gravity
        // ---------------------------------------------

        if (IsGrounded &&
            velocity.y < 0f)
        {
            velocity.y =
                groundedGravity;
        }
        else
        {
            ApplyGravity();
        }


        MoveCharacter();
    }


    // =========================================================
    // JUMP
    // =========================================================

    public void BeginJump()
    {
        // Gameplay-authoritative jump velocity.
        velocity.y =
            jumpForce;


        // Announce that the jump has ACTUALLY started.
        //
        // Camera shake / audio / VFX can react independently.

        JumpStarted?.Invoke();
    }


    public void Land()
    {
        float downwardSpeed =
            Mathf.Max(
                0f,
                -velocity.y
            );


        Landed?.Invoke(
            downwardSpeed
        );
    }


    public void TickAirMovement()
    {
        movementDirection =
            GetCameraRelativeMovementDirection();


        velocity.x =
            movementDirection.x *
            airMovementSpeed;

        velocity.z =
            movementDirection.z *
            airMovementSpeed;


        ApplyGravity();


        MoveCharacter();
    }


    // =========================================================
    // CHARACTER MOVEMENT
    // =========================================================

    private void MoveCharacter()
    {
        characterController.Move(
            velocity *
            Time.deltaTime
        );
    }


    // =========================================================
    // GRAVITY
    // =========================================================

    private void ApplyGravity()
    {
        velocity.y +=
            gravity *
            Time.deltaTime;
    }


    public void SnapToGround()
    {
        if (IsGrounded &&
            velocity.y < 0f)
        {
            velocity.y =
                groundedGravity;
        }
    }


    // =========================================================
    // CAMERA-RELATIVE MOVEMENT
    // =========================================================

    private Vector3 GetCameraRelativeMovementDirection()
    {
        Vector2 input =
            inputReader.MoveInput;


        if (input.sqrMagnitude <= 0.01f)
        {
            return Vector3.zero;
        }


        // Actual rendered Main Camera.
        Vector3 cameraForward =
            cameraTransform.forward;

        Vector3 cameraRight =
            cameraTransform.right;


        // Ignore camera pitch.
        //
        // Looking upward/downward must NOT cause
        // the CharacterController to move vertically.

        cameraForward.y = 0f;
        cameraRight.y = 0f;


        cameraForward.Normalize();
        cameraRight.Normalize();


        Vector3 direction =
            cameraForward *
            input.y
            +
            cameraRight *
            input.x;


        // Prevent diagonal movement from exceeding speed.

        return Vector3.ClampMagnitude(
            direction,
            1f
        );
    }
}   
