public class CoverRightPeekState :
    ICoverPeekState
{
    public void Enter(
        CoverPeekController controller)
    {
        controller.SetPeekSide(
            CoverPeekSide.Right
        );
    }


    public void Tick(
        CoverPeekController controller)
    {
        if (!controller.IsInCover)
        {
            controller.ChangeState(
                controller.NeutralState
            );

            return;
        }


        if (!controller.IsAimHeld)
        {
            controller.ChangeState(
                controller.NeutralState
            );

            return;
        }


        if (!controller.CanPeekRight)
        {
            controller.ChangeState(
                controller.NeutralState
            );

            return;
        }


        CoverPeekSide desired =
            controller.DetermineDesiredPeekSide();


        if (desired ==
            CoverPeekSide.Left)
        {
            controller.ChangeState(
                controller.LeftPeekState
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