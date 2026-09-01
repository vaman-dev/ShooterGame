public class CoverNeutralState :
    ICoverPeekState
{
    public void Enter(
        CoverPeekController controller)
    {
        controller.SetPeekSide(
            CoverPeekSide.None
        );
    }


    public void Tick(
        CoverPeekController controller)
    {
        if (!controller.IsInCover)
            return;


        if (!controller.IsAimHeld)
            return;


        CoverPeekSide desiredSide =
            controller.DetermineDesiredPeekSide();


        if (desiredSide ==
            CoverPeekSide.Left)
        {
            controller.ChangeState(
                controller.LeftPeekState
            );

            return;
        }


        if (desiredSide ==
            CoverPeekSide.Right)
        {
            controller.ChangeState(
                controller.RightPeekState
            );
        }
    }


    public void Exit(
        CoverPeekController controller)
    {
    }


    public bool CanTransitionTo(
        ICoverPeekState next)
    {
        return true;
    }
}