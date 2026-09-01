using UnityEngine;

public class WeaponReloadingState : IWeaponState
{
    private float reloadTimer;

    public void Enter(WeaponController controller)
    {
        reloadTimer = controller.ReloadDuration;
        Debug.Log($"[Weapon FSM] Entered Reloading — duration: {reloadTimer}");
    }

    public void Tick(WeaponController controller)
    {
        reloadTimer -= Time.deltaTime;

        if (reloadTimer <= 0f)
        {
            controller.CompleteReload();
            controller.ChangeState(controller.IdleState);
        }
    }

    public void Exit(WeaponController controller)
    {
        Debug.Log("[Weapon FSM] Exited Reloading");
    }

    // Reload should not be interrupted mid-way by a fire attempt —
    // block transition to Firing while reloading is in progress.
    public bool CanTransitionTo(IWeaponState next) => !(next is WeaponFiringState);
}