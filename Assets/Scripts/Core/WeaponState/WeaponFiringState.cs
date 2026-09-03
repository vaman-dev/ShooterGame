//using UnityEngine;

//public class WeaponFiringState : IWeaponState
//{
//    public void Enter(WeaponController controller)
//    {
//        Debug.Log($"[Weapon FSM] Entered Firing — Ammo before shot: {controller.CurrentAmmo}");
//        controller.FireShot();
//    }

//    public void Tick(WeaponController controller)
//    {
//        // Firing is momentary — return to Idle once the fire-rate cooldown has elapsed
//        if (controller.FireCooldownElapsed)
//        {
//            controller.ChangeState(controller.IdleState);
//        }
//    }

//    public void Exit(WeaponController controller)
//    {
//        Debug.Log("[Weapon FSM] Exited Firing");
//    }

//    public bool CanTransitionTo(IWeaponState next) => true;
//}



using UnityEngine;

public class WeaponFiringState : IWeaponState
{
    public void Enter(WeaponController controller)
    {
        Debug.Log(
            $"[Weapon FSM] Entered Firing — " +
            $"Ammo before shot: {controller.CurrentAmmo}"
        );


        // First shot should happen immediately.
        if (controller.CanFire)
        {
            controller.FireShot();
        }
    }


    public void Tick(WeaponController controller)
    {
        // =====================================================
        // RELOAD HAS PRIORITY
        // =====================================================

        if (controller.InputReader.ConsumeReloadPressed() &&
            controller.CanReload)
        {
            controller.ChangeState(
                controller.ReloadingState
            );

            return;
        }


        // =====================================================
        // MAGAZINE EMPTY
        // =====================================================

        if (controller.IsEmpty)
        {
            controller.ChangeState(
                controller.IdleState
            );

            return;
        }


        // =====================================================
        // TRIGGER RELEASED
        // =====================================================

        if (!controller.InputReader.IsFirePressed)
        {
            controller.ChangeState(
                controller.IdleState
            );

            return;
        }


        // =====================================================
        // SEMI-AUTOMATIC
        // =====================================================

        // First shot already happened in Enter().
        //
        // For semi-auto we now wait for trigger release.
        if (!controller.IsAutomatic)
        {
            return;
        }


        // =====================================================
        // AUTOMATIC FIRE
        // =====================================================

        if (controller.CanFire)
        {
            controller.FireShot();
        }
    }


    public void Exit(WeaponController controller)
    {
        Debug.Log(
            "[Weapon FSM] Exited Firing"
        );
    }


    public bool CanTransitionTo(IWeaponState next)
    {
        return next is WeaponIdleState ||
               next is WeaponReloadingState;
    }
}
