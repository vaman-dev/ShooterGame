public interface ICoverState  // separate interface, since Cover has different rules than Movement
{
    void Enter(PlayerController controller);
    void Tick(PlayerController controller);
    void Exit(PlayerController controller);
}

