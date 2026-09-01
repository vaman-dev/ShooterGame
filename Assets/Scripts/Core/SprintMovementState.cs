public class SprintMovementState : IMovementState
{
    public void Enter(PlayerController controller)
    {
        controller.SnapToGround();
    }

    public void Tick(PlayerController controller)
    {
        // --------------------------------
        // Jump while sprinting
        // --------------------------------

        if (controller.InputReader.ConsumeJumpPressed()
            && controller.IsGrounded)
        {
            controller.ChangeState(controller.JumpState);
            return;
        }

        // --------------------------------
        // Stop sprinting
        // --------------------------------

        if (!controller.InputReader.IsSprintHeld
            || !controller.HasMovementInput)
        {
            controller.ChangeState(controller.NormalState);
            return;
        }

        // --------------------------------
        // Sprint movement
        // --------------------------------

        controller.TickGroundMovement(
            controller.SprintSpeed
        );
    }

    public void Exit(PlayerController controller)
    {
    }

    public bool CanTransitionTo(IMovementState next)
    {
        return true;
    }
}