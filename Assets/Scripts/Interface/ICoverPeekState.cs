public interface ICoverPeekState
{
    void Enter(
        CoverPeekController controller
    );

    void Tick(
        CoverPeekController controller
    );

    void Exit(
        CoverPeekController controller
    );

    bool CanTransitionTo(
        ICoverPeekState next
    );
}