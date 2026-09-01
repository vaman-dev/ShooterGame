using UnityEngine;

public class WeaponIdleState : IWeaponState
{
    public void Enter(WeaponController controller)
    {
        Debug.Log("[Weapon FSM] Entered Idle");
    }

    public void Tick(WeaponController controller)
    {
        if (controller.InputReader.IsFirePressed && controller.CanFire)
        {
            controller.ChangeState(controller.FiringState);
            return;
        }

        if (controller.InputReader.ConsumeReloadPressed() && controller.CanReload)
        {
            controller.ChangeState(controller.ReloadingState);
        }
    }

    public void Exit(WeaponController controller) { }

    public bool CanTransitionTo(IWeaponState next) => true;
}