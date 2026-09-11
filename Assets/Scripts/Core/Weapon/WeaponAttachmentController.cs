using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAttachmentController : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private GameObject riflePrefab;

    [Header("Attachment")]
    [SerializeField] private Transform weaponSocket;

    private GameObject equippedWeapon;

    public bool IsEquipped => equippedWeapon != null;

    public GameObject EquippedWeapon => equippedWeapon;

    public Transform WeaponSocket => weaponSocket;

    private void Update()
    {
        if (Keyboard.current == null ||
            !Keyboard.current.eKey.wasPressedThisFrame)
        {
            return;
        }

        if (IsEquipped)
        {
            UnequipRifle();
            return;
        }

        EquipRifle();
    }

    private void EquipRifle()
    {
        if (weaponSocket == null)
        {
            Debug.LogError(
                "[Weapon Attachment] WeaponSocket is not assigned. Use Tools/TPS/Create Weapon Socket.",
                this
            );

            return;
        }

        if (riflePrefab == null)
        {
            Debug.LogError(
                "[Weapon Attachment] Rifle prefab is not assigned.",
                this
            );

            return;
        }

        equippedWeapon =
            Instantiate(
                riflePrefab,
                weaponSocket,
                false
            );

        equippedWeapon.name = riflePrefab.name;
    }

    private void UnequipRifle()
    {
        if (equippedWeapon == null)
            return;

        Destroy(equippedWeapon);
        equippedWeapon = null;
    }
}
