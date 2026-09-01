using System;
using UnityEngine;

public class CoverPeekController : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private CoverController coverController;

    [SerializeField]
    private CoverDetector coverDetector;

    [SerializeField]
    private PlayerInputReader inputReader;

    [SerializeField]
    private CoverCameraOffsetController coverCameraOffsetController;


    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Peek Input")]

    [Tooltip(
        "Horizontal movement input required to choose " +
        "left/right when both cover edges are available."
    )]
    [SerializeField]
    private float horizontalThreshold = 0.35f;


    // =========================================================
    // STATES
    // =========================================================

    public CoverNeutralState NeutralState
    {
        get;
        private set;
    }


    public CoverLeftPeekState LeftPeekState
    {
        get;
        private set;
    }


    public CoverRightPeekState RightPeekState
    {
        get;
        private set;
    }


    private ICoverPeekState currentState;

    private bool wasAimHeld;


    // =========================================================
    // CURRENT PEEK
    // =========================================================

    public CoverPeekSide CurrentPeekSide
    {
        get;
        private set;
    }


    public bool IsPeeking =>
        CurrentPeekSide == CoverPeekSide.Left ||
        CurrentPeekSide == CoverPeekSide.Right;


    // =========================================================
    // COVER INFORMATION
    // =========================================================

    public bool IsInCover =>
        coverController != null &&
        coverController.IsInCover;


    public bool IsAimHeld =>
        inputReader != null &&
        inputReader.IsAimHeld;


    public bool CanPeekLeft =>
        IsInCover &&
        coverDetector != null &&
        coverDetector.CurrentCover.IsValid &&
        coverDetector.CurrentCover.LeftEdgeOpen;


    public bool CanPeekRight =>
        IsInCover &&
        coverDetector != null &&
        coverDetector.CurrentCover.IsValid &&
        coverDetector.CurrentCover.RightEdgeOpen;


    // =========================================================
    // COVER MOVEMENT OWNERSHIP
    // =========================================================

    /// <summary>
    /// CoverController checks this before performing wall-slide movement.
    ///
    /// When aiming at a valid corner, horizontal input belongs to
    /// the peek system rather than the cover-slide system.
    /// </summary>
    public bool ShouldBlockCoverSlide =>
        IsPeeking ||
        (
            IsAimHeld &&
            (
                CanPeekLeft ||
                CanPeekRight
            )
        );


    // =========================================================
    // EVENTS
    // =========================================================

    public event Action<CoverPeekSide> PeekChanged;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        NeutralState =
            new CoverNeutralState();

        LeftPeekState =
            new CoverLeftPeekState();

        RightPeekState =
            new CoverRightPeekState();
    }


    private void Start()
    {
        ChangeState(
            NeutralState
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        LogAimStateChanges();

        // Peek states only exist while the player
        // is actually attached to cover.

        if (!IsInCover)
        {
            ForceNeutral();

            return;
        }


        currentState?.Tick(
            this
        );
    }


    private void LogAimStateChanges()
    {
        bool aimHeld = IsAimHeld;

        if (aimHeld == wasAimHeld)
            return;

        wasAimHeld = aimHeld;

        Debug.Log(
            $"[Cover Peek] Aim {(aimHeld ? "pressed" : "released")} | " +
            $"InCover={IsInCover}, ValidCover={coverDetector != null && coverDetector.CurrentCover.IsValid}, " +
            $"LeftOpen={CanPeekLeft}, RightOpen={CanPeekRight}, " +
            $"HorizontalInput={(inputReader != null ? inputReader.MoveInput.x : 0f):F2}",
            this
        );
    }


    // =========================================================
    // STATE MACHINE
    // =========================================================

    public void ChangeState(
        ICoverPeekState nextState)
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


        currentState?.Exit(
            this
        );


        currentState =
            nextState;


        currentState.Enter(
            this
        );
    }


    // =========================================================
    // PEEK DECISION
    // =========================================================

    public CoverPeekSide DetermineDesiredPeekSide()
    {
        if (!IsAimHeld)
        {
            return CoverPeekSide.None;
        }


        bool leftAvailable =
            CanPeekLeft;


        bool rightAvailable =
            CanPeekRight;


        // =====================================================
        // ONLY LEFT EDGE AVAILABLE
        // =====================================================

        if (leftAvailable &&
            !rightAvailable)
        {
            return CoverPeekSide.Left;
        }


        // =====================================================
        // ONLY RIGHT EDGE AVAILABLE
        // =====================================================

        if (rightAvailable &&
            !leftAvailable)
        {
            return CoverPeekSide.Right;
        }


        // =====================================================
        // BOTH EDGES AVAILABLE
        // =====================================================

        if (leftAvailable &&
            rightAvailable)
        {
            float horizontalInput =
                inputReader.MoveInput.x;


            if (horizontalInput <
                -horizontalThreshold)
            {
                return CoverPeekSide.Left;
            }


            if (horizontalInput >
                horizontalThreshold)
            {
                return CoverPeekSide.Right;
            }
        }


        return CoverPeekSide.None;
    }


    // =========================================================
    // SET CURRENT PEEK
    // =========================================================

    public void SetPeekSide(
        CoverPeekSide side)
    {
        if (CurrentPeekSide == side)
            return;


        CurrentPeekSide =
            side;


        if (side == CoverPeekSide.Left ||
            side == CoverPeekSide.Right)
        {
            Debug.Log(
                $"[Cover Peek] Entered {side} peek mode.",
                this
            );
        }
        else
        {
            Debug.Log(
                "[Cover Peek] Exited peek mode.",
                this
            );
        }


        // Camera presentation reacts to gameplay state.
        if (coverCameraOffsetController != null)
        {
            coverCameraOffsetController.SetPeekSide(
                side
            );
        }


        PeekChanged?.Invoke(
            side
        );
    }


    // =========================================================
    // RESET
    // =========================================================

    public void ForceNeutral()
    {
        if (ReferenceEquals(
            currentState,
            NeutralState))
        {
            SetPeekSide(
                CoverPeekSide.None
            );

            return;
        }


        ChangeState(
            NeutralState
        );
    }
}
