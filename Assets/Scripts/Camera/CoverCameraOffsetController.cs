using UnityEngine;

public class CoverCameraOffsetController :
    MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private Transform coverCameraTarget;


    // =========================================================
    // OFFSETS
    // =========================================================

    [Header("Camera Target Offsets")]

    [SerializeField]
    private Vector3 neutralOffset =
        Vector3.zero;


    [SerializeField]
    private Vector3 leftPeekOffset =
        new Vector3(
            -0.55f,
            0.05f,
            0f
        );


    [SerializeField]
    private Vector3 rightPeekOffset =
        new Vector3(
            0.55f,
            0.05f,
            0f
        );


    // =========================================================
    // SMOOTHING
    // =========================================================

    [Header("Smoothing")]

    [SerializeField]
    private float offsetSharpness = 15f;


    // =========================================================
    // RUNTIME
    // =========================================================

    private Vector3 targetOffset;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        if (coverCameraTarget == null)
            return;


        if (neutralOffset ==
            Vector3.zero)
        {
            neutralOffset =
                coverCameraTarget.localPosition;
        }


        targetOffset =
            neutralOffset;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void LateUpdate()
    {
        if (coverCameraTarget == null)
            return;


        float t =
            1f -
            Mathf.Exp(
                -offsetSharpness *
                Time.deltaTime
            );


        coverCameraTarget.localPosition =
            Vector3.Lerp(
                coverCameraTarget.localPosition,
                targetOffset,
                t
            );
    }


    // =========================================================
    // SIDE
    // =========================================================

    public void SetPeekSide(
        CoverPeekSide side)
    {
        switch (side)
        {
            case CoverPeekSide.Left:

                targetOffset =
                    leftPeekOffset;

                break;


            case CoverPeekSide.Right:

                targetOffset =
                    rightPeekOffset;

                break;


            default:

                targetOffset =
                    neutralOffset;

                break;
        }
    }
}