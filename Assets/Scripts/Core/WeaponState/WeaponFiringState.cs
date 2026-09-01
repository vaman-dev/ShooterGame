using UnityEngine;

public class WeaponFiringState : IWeaponState
{
    public void Enter(WeaponController controller)
    {
        Debug.Log($"[Weapon FSM] Entered Firing — Ammo before shot: {controller.CurrentAmmo}");
        controller.FireShot();
    }

    public void Tick(WeaponController controller)
    {
        // Firing is momentary — return to Idle once the fire-rate cooldown has elapsed
        if (controller.FireCooldownElapsed)
        {
            controller.ChangeState(controller.IdleState);
        }
    }

    public void Exit(WeaponController controller)
    {
        Debug.Log("[Weapon FSM] Exited Firing");
    }

    public bool CanTransitionTo(IWeaponState next) => true;
}