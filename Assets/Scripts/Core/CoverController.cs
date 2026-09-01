//using UnityEngine;

//[RequireComponent(typeof(CoverDetector))]
//[RequireComponent(typeof(PlayerInputReader))]
//public class CoverController : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private CoverDetector coverDetector;
//    [SerializeField] private PlayerInputReader inputReader;
//    [SerializeField] private CharacterController characterController;
//    [SerializeField] private Transform root; // PlayerRoot

//    [Header("Cover Settings")]
//    [Tooltip("Extra buffer distance beyond the capsule radius, so it doesn't touch-clip the wall.")]
//    [SerializeField] private float wallBuffer = 0.05f;

//    [SerializeField] private float snapRotationSpeed = 12f;

//    public bool IsInCover { get; private set; }

//    private void Awake()
//    {
//        if (coverDetector == null)
//            coverDetector = GetComponent<CoverDetector>();

//        if (inputReader == null)
//            inputReader = GetComponent<PlayerInputReader>();

//        if (characterController == null)
//            characterController = GetComponent<CharacterController>();
//    }

//    private void Update()
//    {
//        if (inputReader.ConsumeCoverPressed())
//        {
//            ToggleCover();
//        }

//        if (IsInCover)
//        {
//            HoldAtCover();
//        }
//    }

//    private void ToggleCover()
//    {
//        if (IsInCover)
//        {
//            ExitCover();
//            return;
//        }

//        if (coverDetector.TryDetectCover(out CoverDetector.CoverInfo info))
//        {
//            EnterCover(info);
//        }
//    }

//    private void EnterCover(CoverDetector.CoverInfo info)
//    {
//        IsInCover = true;
//        SnapToWall(info);
//    }

//    private void ExitCover()
//    {
//        IsInCover = false;
//    }

//    private void HoldAtCover()
//    {
//        if (!coverDetector.TryDetectCover(out CoverDetector.CoverInfo info))
//        {
//            // Wall no longer detected — auto-exit rather than floating in space
//            ExitCover();
//            return;
//        }

//        SnapToWall(info);
//    }

//    private void SnapToWall(CoverDetector.CoverInfo info)
//    {
//        // Correct distance from wall = capsule radius + small buffer,
//        // measured outward along the wall's normal from the actual hit point.
//        float standDistance = characterController.radius + wallBuffer;
//        Vector3 targetPosition = info.Point + info.Normal * standDistance;
//        targetPosition.y = root.position.y; // preserve vertical/gravity state

//        // Disable/re-enable CharacterController to safely teleport it —
//        // directly setting transform.position while CC is enabled
//        // can be silently resisted or overridden on the next Move() call.
//        characterController.enabled = false;
//        root.position = targetPosition;
//        characterController.enabled = true;

//        // Face along the wall, back to the surface (standard cover orientation)
//        Quaternion targetRotation = Quaternion.LookRotation(-info.Normal, Vector3.up);
//        root.rotation = Quaternion.Slerp(root.rotation, targetRotation, snapRotationSpeed * Time.deltaTime);
//    }
//}

using UnityEngine;

[RequireComponent(typeof(CoverDetector))]
[RequireComponent(typeof(PlayerInputReader))]
public class CoverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CoverDetector coverDetector;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform root; // PlayerRoot
    [SerializeField] private CoverPeekController coverPeekController;

    [Header("Cover Settings")]
    [Tooltip("Extra buffer distance beyond the capsule radius, so it doesn't touch-clip the wall.")]
    [SerializeField] private float wallBuffer = 0.05f;

    [Tooltip("Sideways slide speed while in cover.")]
    [SerializeField] private float slideSpeed = 2.5f;

    [Tooltip("How often (seconds) to re-check the wall is still there / edges are still valid, without re-teleporting.")]
    [SerializeField] private float recheckInterval = 0.15f;

    public bool IsInCover { get; private set; }

    private Vector3 wallNormal;
    private Vector3 wallTangent;
    private float recheckTimer;

    private void Awake()
    {
        if (coverDetector == null)
            coverDetector = GetComponent<CoverDetector>();

        if (inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (coverPeekController == null)
            coverPeekController = GetComponent<CoverPeekController>();
    }

    private void Update()
    {
        if (inputReader.ConsumeCoverPressed())
        {
            ToggleCover();
        }

        if (IsInCover)
        {
            if (coverPeekController == null ||
                !coverPeekController.ShouldBlockCoverSlide)
            {
                TickCoverMovement();
            }

            TickWallStillValid();
        }
    }

    // =========================================================
    // ENTER / EXIT
    // =========================================================

    private void ToggleCover()
    {
        if (IsInCover)
        {
            ExitCover();
            return;
        }

        if (coverDetector.TryDetectCover(out CoverDetector.CoverInfo info))
        {
            EnterCover(info);
        }
    }

    private void EnterCover(CoverDetector.CoverInfo info)
    {
        IsInCover = true;

        wallNormal = info.Normal;
        // Match CoverDetector's wall-right axis so positive horizontal input
        // moves toward, and peeks from, the right cover edge.
        wallTangent = Vector3.Cross(wallNormal, Vector3.up).normalized;

        SnapToWallOnce(info.Point);
        LockRotationToWall();

        recheckTimer = 0f;
    }

    private void ExitCover()
    {
        IsInCover = false;
    }

    // =========================================================
    // ONE-TIME SNAP (entry only, not per-frame)
    // =========================================================

    private void SnapToWallOnce(Vector3 hitPoint)
    {
        float standDistance = characterController.radius + wallBuffer;
        Vector3 targetPosition = hitPoint + wallNormal * standDistance;
        targetPosition.y = root.position.y;

        characterController.enabled = false;
        root.position = targetPosition;
        characterController.enabled = true;
    }

    private void LockRotationToWall()
    {
        root.rotation = Quaternion.LookRotation(-wallNormal, Vector3.up);
    }

    // =========================================================
    // SLIDE MOVEMENT (tangent only — no per-frame re-teleport)
    // =========================================================

    private void TickCoverMovement()
    {
        Vector2 input = inputReader.MoveInput;

        // Only the LEFT/RIGHT (input.x) component matters in cover —
        // forward/back input (input.y) is ignored, since moving into
        // or away from the wall isn't valid while sticking to cover.
        Vector3 slideDirection = wallTangent * input.x;

        Vector3 motion = slideDirection * slideSpeed * Time.deltaTime;

        // Keep the player pinned at the correct distance from the wall
        // by also nudging back onto the wall plane — this corrects small
        // drift from collision resolution WITHOUT teleporting every frame.
        Vector3 toWall = root.position - (root.position - wallNormal * Vector3.Dot(root.position, wallNormal));
        motion += -wallNormal * Vector3.Dot(characterController.velocity, wallNormal) * Time.deltaTime;

        // Simple grounded gravity so the player doesn't float while sliding.
        motion += Physics.gravity * Time.deltaTime;

        characterController.Move(motion);

        // Rotation stays locked to the wall the whole time in cover —
        // CameraRigController must NOT rotate root while this is true (see fix below).
        LockRotationToWall();
    }

    // =========================================================
    // PERIODIC VALIDITY CHECK (not a re-snap, just an exit condition)
    // =========================================================

    private void TickWallStillValid()
    {
        recheckTimer += Time.deltaTime;

        if (recheckTimer < recheckInterval)
            return;

        recheckTimer = 0f;

        // Short ray straight out along the stored wall normal (not transform.forward,
        // since transform.forward is now locked to face away from the wall anyway).
        bool stillValid = Physics.Raycast(
            root.position,
            -wallNormal,
            out RaycastHit hit,
            characterController.radius + wallBuffer + 0.3f,
            coverDetector.CoverLayerMaskPublic
        );

        if (!stillValid)
        {
            ExitCover();
        }
    }
}
