using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponLeftHandIKController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponAttachmentController weaponAttachmentController;
    [SerializeField] private Rig weaponRig;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandIKTarget;
    [SerializeField] private Transform leftElbowHint;

    [Header("Runtime Blend")]
    [SerializeField, Min(0.01f)] private float blendDuration = 0.2f;

    private Transform activeGrip;
    private float currentWeight;

    private void Awake()
    {
        if (weaponAttachmentController == null)
            weaponAttachmentController = GetComponent<WeaponAttachmentController>();
    }

    private void LateUpdate()
    {
        if (weaponRig == null || leftHandIK == null || leftHandIKTarget == null)
            return;

        bool hasWeapon = weaponAttachmentController != null &&
            weaponAttachmentController.IsEquipped;

        Transform grip = hasWeapon ? FindGrip(weaponAttachmentController.EquippedWeapon) : null;

        if (grip != null)
        {
            activeGrip = grip;
            leftHandIKTarget.SetPositionAndRotation(grip.position, grip.rotation);
        }
        else
        {
            activeGrip = null;
        }

        float targetWeight = activeGrip != null ? 1f : 0f;
        currentWeight = Mathf.MoveTowards(
            currentWeight,
            targetWeight,
            Time.deltaTime / Mathf.Max(0.01f, blendDuration));

        weaponRig.weight = currentWeight;
        leftHandIK.weight = currentWeight;
    }

    private static Transform FindGrip(GameObject weapon)
    {
        if (weapon == null)
            return null;

        Transform[] transforms = weapon.GetComponentsInChildren<Transform>(true);
        foreach (Transform candidate in transforms)
        {
            if (candidate.name == "LeftHandGrip")
                return candidate;
        }

        return null;
    }
}
