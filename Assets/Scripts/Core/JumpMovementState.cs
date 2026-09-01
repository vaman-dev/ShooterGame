public class JumpMovementState : IMovementState
{
    public void Enter(PlayerController controller)
    {
        controller.BeginJump();
    }

    public void Tick(PlayerController controller)
    {
        controller.TickAirMovement();

        if (controller.IsGrounded &&
            controller.VerticalVelocity <= 0f)
        {
            controller.Land();
            controller.ChangeState(controller.NormalState);
        }
    }

    public void Exit(PlayerController controller)
    {
    }

    public bool CanTransitionTo(IMovementState next)
    {
        return true;
    }
}
