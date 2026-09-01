using UnityEngine;
using Unity.Cinemachine;

public class AimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera aimCamera;       // AimCam
    [SerializeField] private Transform fallbackCameraTransform; // TPS_FollowCam's transform, for hip-fire raycast

    [Header("Settings")]
    [SerializeField] private float maxAimDistance = 100f;
    [SerializeField] private LayerMask aimLayerMask = ~0; // everything by default, refine later

    private CinemachineThirdPersonAim thirdPersonAim;

    public Vector3 CurrentAimPoint { get; private set; }
    public Vector3 CurrentAimDirection { get; private set; }

    private void Awake()
    {
        thirdPersonAim = aimCamera.GetComponent<CinemachineThirdPersonAim>();

        if (thirdPersonAim == null)
        {
            Debug.LogWarning("AimController: CinemachineThirdPersonAim not found on AimCam.");
        }
    }

    private void Update()
    {
        UpdateAimPoint();
        DrawDebugRay();
    }

    private void UpdateAimPoint()
    {
        if (thirdPersonAim != null)
        {
            // AimTarget is the corrected hit point Cinemachine already calculated
            CurrentAimPoint = thirdPersonAim.AimTarget;
            CurrentAimDirection = (CurrentAimPoint - fallbackCameraTransform.position).normalized;
        }
        else
        {
            // Fallback: raw raycast from camera forward, for hip-fire or if extension is missing
            Vector3 origin = fallbackCameraTransform.position;
            Vector3 direction = fallbackCameraTransform.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxAimDistance, aimLayerMask))
            {
                CurrentAimPoint = hit.point;
            }
            else
            {
                CurrentAimPoint = origin + direction * maxAimDistance;
            }

            CurrentAimDirection = direction;
        }
    }

    private void DrawDebugRay()
    {
        Debug.DrawRay(fallbackCameraTransform.position, CurrentAimDirection * maxAimDistance, Color.red);
        Debug.DrawLine(fallbackCameraTransform.position, CurrentAimPoint, Color.green);
    }
}