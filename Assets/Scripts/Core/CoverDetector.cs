//using UnityEngine;

//[RequireComponent(typeof(CharacterController))]
//public class CoverDetector : MonoBehaviour
//{
//    // =========================================================
//    // REFERENCES
//    // =========================================================

//    [Header("References")]
//    [SerializeField] private CharacterController characterController;

//    [Tooltip("Optional manual override point for the raycast origin. " +
//             "If left empty, origin is derived from CharacterController's center automatically.")]
//    [SerializeField] private Transform manualRaycastOrigin;

//    // =========================================================
//    // DETECTION SETTINGS
//    // =========================================================

//    [Header("Detection")]
//    [SerializeField] private float detectionRadius = 1f;
//    [SerializeField] private float sphereCastRadius = 0.3f;
//    [SerializeField] private LayerMask coverLayerMask;

//    [Tooltip("How far left/right to check from the front hit point, for corner/peek edge detection.")]
//    [SerializeField] private float sideCheckOffset = 0.6f;

//    // =========================================================
//    // RESULT DATA
//    // =========================================================

//    public struct CoverInfo
//    {
//        public bool IsValid;
//        public Vector3 Point;
//        public Vector3 Normal;
//        public bool LeftEdgeOpen;
//        public bool RightEdgeOpen;
//    }

//    public CoverInfo CurrentCover { get; private set; }
//    // Add this public property to CoverDetector.cs
//    public LayerMask CoverLayerMaskPublic => coverLayerMask;

//    // =========================================================
//    // INIT
//    // =========================================================

//    private void Awake()
//    {
//        if (characterController == null)
//            characterController = GetComponent<CharacterController>();
//    }

//    // =========================================================
//    // UPDATE
//    // =========================================================

//    private void Update()
//    {
//        TryDetectCover(out CoverInfo _);
//    }

//    // =========================================================
//    // ORIGIN RESOLUTION
//    // =========================================================

//    /// <summary>
//    /// Returns the world-space point the cover raycasts should originate from.
//    /// Prefers a manually assigned point (for precise designer control),
//    /// falls back to CharacterController's actual collider center otherwise —
//    /// never assumes transform.position is correct, since CC.Center can be offset.
//    /// </summary>
//    private Vector3 GetRaycastOrigin()
//    {
//        if (manualRaycastOrigin != null)
//            return manualRaycastOrigin.position;

//        // characterController.bounds.center reflects the REAL world-space
//        // center of the capsule, already accounting for Center/Height/scale.
//        return characterController.bounds.center;
//    }

//    // =========================================================
//    // CORE DETECTION
//    // =========================================================

//    public bool TryDetectCover(out CoverInfo result)
//    {
//        Vector3 origin = GetRaycastOrigin();
//        Vector3 forward = transform.forward;

//        bool hitFront = Physics.SphereCast(
//            origin,
//            sphereCastRadius,
//            forward,
//            out RaycastHit frontHit,
//            detectionRadius,
//            coverLayerMask
//        );

//        if (!hitFront)
//        {
//            result = default;
//            CurrentCover = result;
//            return false;
//        }

//        // Check left/right from the wall-hit point to see if this is
//        // a corner (peek-able edge) or a flat/enclosed wall.
//        bool leftOpen = !Physics.Raycast(
//            frontHit.point + transform.right * -sideCheckOffset + Vector3.up * 0.1f,
//            forward,
//            0.5f,
//            coverLayerMask
//        );

//        bool rightOpen = !Physics.Raycast(
//            frontHit.point + transform.right * sideCheckOffset + Vector3.up * 0.1f,
//            forward,
//            0.5f,
//            coverLayerMask
//        );

//        result = new CoverInfo
//        {
//            IsValid = true,
//            Point = frontHit.point,
//            Normal = frontHit.normal,
//            LeftEdgeOpen = leftOpen,
//            RightEdgeOpen = rightOpen
//        };

//        CurrentCover = result;
//        Debug.Log($"Cover detected at {CurrentCover.Point}");
//        return true;
//    }

//    // =========================================================
//    // DEBUG VISUALIZATION
//    // =========================================================

//    private void OnDrawGizmosSelected()
//    {
//        Vector3 origin = Application.isPlaying
//            ? GetRaycastOrigin()
//            : transform.position + Vector3.up * 1f; // rough preview in edit mode

//        Gizmos.color = Color.cyan;
//        Gizmos.DrawWireSphere(origin, sphereCastRadius);
//        Gizmos.DrawLine(origin, origin + transform.forward * detectionRadius);

//        if (CurrentCover.IsValid)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawSphere(CurrentCover.Point, 0.1f);
//            Gizmos.DrawLine(CurrentCover.Point, CurrentCover.Point + CurrentCover.Normal * 0.5f);
//        }
//    }
//}


using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CoverDetector : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]
    [SerializeField]
    private CharacterController characterController;

    [Tooltip(
        "Optional custom cover-detection origin. " +
        "If empty, CharacterController bounds center is used."
    )]
    [SerializeField]
    private Transform manualRaycastOrigin;


    // =========================================================
    // DETECTION
    // =========================================================

    [Header("Detection")]

    [SerializeField]
    private float detectionRadius = 1f;

    [SerializeField]
    private float sphereCastRadius = 0.3f;

    [SerializeField]
    private LayerMask coverLayerMask;


    // =========================================================
    // EDGE DETECTION
    // =========================================================

    [Header("Edge Detection")]

    [Tooltip("Distance left/right from the detected cover point.")]
    [SerializeField]
    private float sideCheckOffset = 0.6f;

    [Tooltip("Distance to raycast back toward the wall.")]
    [SerializeField]
    private float edgeProbeDistance = 0.6f;

    [Tooltip(
        "Moves the probe slightly away from the wall before raycasting back."
    )]
    [SerializeField]
    private float edgeProbeOutset = 0.05f;

    [SerializeField]
    private float edgeProbeHeight = 0.1f;


    // =========================================================
    // RESULT
    // =========================================================

    public struct CoverInfo
    {
        public bool IsValid;

        public Vector3 Point;

        public Vector3 Normal;

        public bool LeftEdgeOpen;

        public bool RightEdgeOpen;
    }


    public CoverInfo CurrentCover
    {
        get;
        private set;
    }


    public LayerMask CoverLayerMaskPublic =>
        coverLayerMask;


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
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        TryDetectCover(
            out _
        );
    }


    // =========================================================
    // ORIGIN
    // =========================================================

    private Vector3 GetRaycastOrigin()
    {
        if (manualRaycastOrigin != null)
        {
            return manualRaycastOrigin.position;
        }


        return characterController.bounds.center;
    }


    // =========================================================
    // COVER DETECTION
    // =========================================================

    public bool TryDetectCover(
        out CoverInfo result)
    {
        Vector3 origin =
            GetRaycastOrigin();


        Vector3 forward =
            Vector3.ProjectOnPlane(
                transform.forward,
                Vector3.up
            );


        if (forward.sqrMagnitude < 0.001f)
        {
            result = default;

            CurrentCover = result;

            return false;
        }


        forward.Normalize();


        bool hitFront =
            Physics.SphereCast(
                origin,
                sphereCastRadius,
                forward,
                out RaycastHit frontHit,
                detectionRadius,
                coverLayerMask,
                QueryTriggerInteraction.Ignore
            );

        // Cyan = front cover found; red = no front cover.
        Debug.DrawRay(
            origin,
            forward * detectionRadius,
            hitFront ? Color.cyan : Color.red
        );


        if (!hitFront)
        {
            result = default;

            CurrentCover = result;

            return false;
        }


        // ---------------------------------------------
        // WALL NORMAL
        // ---------------------------------------------

        Vector3 wallNormal =
            Vector3.ProjectOnPlane(
                frontHit.normal,
                Vector3.up
            );


        if (wallNormal.sqrMagnitude < 0.001f)
        {
            wallNormal =
                frontHit.normal;
        }


        wallNormal.Normalize();


        // ---------------------------------------------
        // WALL TANGENT
        // ---------------------------------------------
        //
        // Gives a stable left/right axis along the wall.

        Vector3 wallRight =
            Vector3.Cross(
                wallNormal,
                Vector3.up
            ).normalized;


        // ---------------------------------------------
        // EDGE PROBES
        // ---------------------------------------------

        Vector3 probeBase =
            frontHit.point +
            wallNormal * edgeProbeOutset +
            Vector3.up * edgeProbeHeight;


        Vector3 leftOrigin =
            probeBase -
            wallRight * sideCheckOffset;


        Vector3 rightOrigin =
            probeBase +
            wallRight * sideCheckOffset;


        Vector3 towardWall =
            -wallNormal;


        bool wallExistsLeft =
            Physics.Raycast(
                leftOrigin,
                towardWall,
                edgeProbeDistance,
                coverLayerMask,
                QueryTriggerInteraction.Ignore
            );


        bool wallExistsRight =
            Physics.Raycast(
                rightOrigin,
                towardWall,
                edgeProbeDistance,
                coverLayerMask,
                QueryTriggerInteraction.Ignore
            );

        // Green = that side is open and can be peeked.
        // Red = cover still exists there, so that side cannot be peeked.
        Debug.DrawRay(
            leftOrigin,
            towardWall * edgeProbeDistance,
            wallExistsLeft ? Color.red : Color.green
        );

        Debug.DrawRay(
            rightOrigin,
            towardWall * edgeProbeDistance,
            wallExistsRight ? Color.red : Color.green
        );

        // Vertical markers identify the actual left/right probe origins.
        Debug.DrawRay(leftOrigin, Vector3.up * 0.15f, Color.yellow);
        Debug.DrawRay(rightOrigin, Vector3.up * 0.15f, Color.magenta);


        result =
            new CoverInfo
            {
                IsValid = true,

                Point =
                    frontHit.point,

                Normal =
                    wallNormal,

                LeftEdgeOpen =
                    !wallExistsLeft,

                RightEdgeOpen =
                    !wallExistsRight
            };


        CurrentCover =
            result;


        return true;
    }


    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Vector3 origin =
            Application.isPlaying
                ? GetRaycastOrigin()
                : transform.position +
                  Vector3.up;


        Gizmos.color =
            Color.cyan;


        Gizmos.DrawWireSphere(
            origin,
            sphereCastRadius
        );


        Gizmos.DrawLine(
            origin,
            origin +
            transform.forward *
            detectionRadius
        );


        if (!CurrentCover.IsValid)
            return;


        Gizmos.color =
            Color.green;


        Gizmos.DrawSphere(
            CurrentCover.Point,
            0.08f
        );


        Gizmos.DrawLine(
            CurrentCover.Point,
            CurrentCover.Point +
            CurrentCover.Normal *
            0.5f
        );
    }
}
