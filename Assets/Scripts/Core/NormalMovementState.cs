public class NormalMovementState : IMovementState
{
    public void Enter(PlayerController controller)
    {
        controller.SnapToGround();
    }

    public void Tick(PlayerController controller)
    {
        // --------------------------------
        // Jump
        // --------------------------------

        if (controller.InputReader.ConsumeJumpPressed()
            && controller.IsGrounded)
        {
            controller.ChangeState(controller.JumpState);
            return;
        }

        // --------------------------------
        // Sprint
        // --------------------------------

        if (controller.InputReader.IsSprintHeld
            && controller.HasMovementInput)
        {
            controller.ChangeState(controller.SprintState);
            return;
        }

        // --------------------------------
        // Normal movement
        // --------------------------------

        controller.TickGroundMovement(
            controller.WalkSpeed
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