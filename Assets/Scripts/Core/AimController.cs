//using UnityEngine;
//using Unity.Cinemachine;

//public class AimController : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private CinemachineCamera aimCamera;       // AimCam
//    [SerializeField] private Transform fallbackCameraTransform; // TPS_FollowCam's transform, for hip-fire raycast

//    [Header("Settings")]
//    [SerializeField] private float maxAimDistance = 100f;
//    [SerializeField] private LayerMask aimLayerMask = ~0; // everything by default, refine later

//    private CinemachineThirdPersonAim thirdPersonAim;

//    public Vector3 CurrentAimPoint { get; private set; }
//    public Vector3 CurrentAimDirection { get; private set; }

//    private void Awake()
//    {
//        thirdPersonAim = aimCamera.GetComponent<CinemachineThirdPersonAim>();

//        if (thirdPersonAim == null)
//        {
//            Debug.LogWarning("AimController: CinemachineThirdPersonAim not found on AimCam.");
//        }
//    }

//    private void Update()
//    {
//        UpdateAimPoint();
//        DrawDebugRay();
//    }

//    private void UpdateAimPoint()
//    {
//        if (thirdPersonAim != null)
//        {
//            // AimTarget is the corrected hit point Cinemachine already calculated
//            CurrentAimPoint = thirdPersonAim.AimTarget;
//            CurrentAimDirection = (CurrentAimPoint - fallbackCameraTransform.position).normalized;
//        }
//        else
//        {
//            // Fallback: raw raycast from camera forward, for hip-fire or if extension is missing
//            Vector3 origin = fallbackCameraTransform.position;
//            Vector3 direction = fallbackCameraTransform.forward;

//            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxAimDistance, aimLayerMask))
//            {
//                CurrentAimPoint = hit.point;
//            }
//            else
//            {
//                CurrentAimPoint = origin + direction * maxAimDistance;
//            }

//            CurrentAimDirection = direction;
//        }
//    }

//    private void DrawDebugRay()
//    {
//        Debug.DrawRay(fallbackCameraTransform.position, CurrentAimDirection * maxAimDistance, Color.red);
//        Debug.DrawLine(fallbackCameraTransform.position, CurrentAimPoint, Color.green);
//    }
//}


using UnityEngine;

public class AimController : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [Tooltip("The real Unity Main Camera controlled by Cinemachine Brain.")]
    [SerializeField]
    private Camera mainCamera;


    // =========================================================
    // AIM SETTINGS
    // =========================================================

    [Header("Aim Settings")]

    [SerializeField]
    private float maxAimDistance = 100f;

    [Tooltip(
        "Usually: Environment + Cover + Target. " +
        "Do NOT include Player."
    )]
    [SerializeField]
    private LayerMask aimLayerMask;


    // =========================================================
    // DEBUG
    // =========================================================

    [Header("Debug")]

    [SerializeField]
    private bool showAimDebug = true;


    // =========================================================
    // PUBLIC DATA
    // =========================================================

    public Vector3 CurrentAimPoint
    {
        get;
        private set;
    }


    public Vector3 CurrentAimDirection
    {
        get;
        private set;
    }


    public bool HasAimHit
    {
        get;
        private set;
    }


    public RaycastHit CurrentAimHit
    {
        get;
        private set;
    }


    public float MaxAimDistance =>
        maxAimDistance;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        ResolveAim();

        if (showAimDebug)
        {
            DrawDebugAim();
        }
    }


    // =========================================================
    // AIM RESOLUTION
    // =========================================================

    public void ResolveAim()
    {
        if (mainCamera == null)
            return;


        // Exact center of the player's screen/crosshair.
        Ray aimRay =
            mainCamera.ViewportPointToRay(
                new Vector3(
                    0.5f,
                    0.5f,
                    0f
                )
            );


        HasAimHit =
            Physics.Raycast(
                aimRay,
                out RaycastHit hit,
                maxAimDistance,
                aimLayerMask,
                QueryTriggerInteraction.Ignore
            );


        if (HasAimHit)
        {
            CurrentAimHit =
                hit;

            CurrentAimPoint =
                hit.point;
        }
        else
        {
            CurrentAimPoint =
                aimRay.origin +
                aimRay.direction *
                maxAimDistance;
        }


        Vector3 direction =
            CurrentAimPoint -
            aimRay.origin;


        if (direction.sqrMagnitude > 0.001f)
        {
            CurrentAimDirection =
                direction.normalized;
        }
        else
        {
            CurrentAimDirection =
                mainCamera.transform.forward;
        }
    }


    // =========================================================
    // DEBUG
    // =========================================================

    private void DrawDebugAim()
    {
        if (mainCamera == null)
            return;


        Debug.DrawLine(
            mainCamera.transform.position,
            CurrentAimPoint,
            Color.green
        );
    }
}