public interface IWeaponState
{
    void Enter(WeaponController controller);
    void Tick(WeaponController controller);
    void Exit(WeaponController controller);
    bool CanTransitionTo(IWeaponState next);
}