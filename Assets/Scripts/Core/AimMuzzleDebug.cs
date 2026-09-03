using UnityEngine;

public class AimMuzzleDebug : MonoBehaviour
{
    [SerializeField]
    private AimController aimController;

    [SerializeField]
    private Transform muzzlePoint;

    [SerializeField]
    private LayerMask hitMask = ~0;


    private void Update()
    {
        if (aimController == null ||
            muzzlePoint == null)
        {
            return;
        }


        Vector3 target =
            aimController.CurrentAimPoint;


        Vector3 direction =
            target -
            muzzlePoint.position;


        float distance =
            direction.magnitude;


        if (distance <= 0.001f)
            return;


        direction.Normalize();


        if (Physics.Raycast(
            muzzlePoint.position,
            direction,
            out RaycastHit hit,
            distance,
            hitMask,
            QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(
                muzzlePoint.position,
                hit.point,
                Color.red
            );
        }
        else
        {
            Debug.DrawLine(
                muzzlePoint.position,
                target,
                Color.red
            );
        }
    }
}