public class CoverLeftPeekState :
    ICoverPeekState
{
    public void Enter(
        CoverPeekController controller)
    {
        controller.SetPeekSide(
            CoverPeekSide.Left
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


        if (!controller.CanPeekLeft)
        {
            controller.ChangeState(
                controller.NeutralState
            );

            return;
        }


        CoverPeekSide desired =
            controller.DetermineDesiredPeekSide();


        if (desired ==
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