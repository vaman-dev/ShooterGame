public interface IMovementState
{
    void Enter(PlayerController controller);

    void Tick(PlayerController controller);

    void Exit(PlayerController controller);

    bool CanTransitionTo(IMovementState next);
}