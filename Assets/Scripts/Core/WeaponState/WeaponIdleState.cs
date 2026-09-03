//using UnityEngine;

//public class WeaponIdleState : IWeaponState
//{
//    public void Enter(WeaponController controller)
//    {
//        Debug.Log("[Weapon FSM] Entered Idle");
//    }

//    public void Tick(WeaponController controller)
//    {
//        if (controller.InputReader.IsFirePressed && controller.CanFire)
//        {
//            controller.ChangeState(controller.FiringState);
//            return;
//        }

//        if (controller.InputReader.ConsumeReloadPressed() && controller.CanReload)
//        {
//            controller.ChangeState(controller.ReloadingState);
//        }
//    }

//    public void Exit(WeaponController controller) { }

//    public bool CanTransitionTo(IWeaponState next) => true;
//}


using UnityEngine;

public class WeaponIdleState : IWeaponState
{
    private bool emptyFeedbackSent;


    public void Enter(WeaponController controller)
    {
        emptyFeedbackSent = false;

        Debug.Log(
            "[Weapon FSM] Entered Idle"
        );
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
        // NO FIRE INPUT
        // =====================================================

        if (!controller.InputReader.IsFirePressed)
        {
            emptyFeedbackSent = false;

            return;
        }


        // =====================================================
        // EMPTY MAGAZINE
        // =====================================================

        if (controller.IsEmpty)
        {
            if (!emptyFeedbackSent)
            {
                controller.NotifyFireBlockedNoAmmo();

                emptyFeedbackSent = true;
            }

            return;
        }


        // =====================================================
        // START FIRING SESSION
        // =====================================================

        if (controller.FireCooldownElapsed)
        {
            controller.ChangeState(
                controller.FiringState
            );
        }
    }


    public void Exit(WeaponController controller)
    {
        Debug.Log(
            "[Weapon FSM] Exited Idle"
        );
    }


    public bool CanTransitionTo(IWeaponState next)
    {
        return next is WeaponFiringState ||
               next is WeaponReloadingState;
    }
}
