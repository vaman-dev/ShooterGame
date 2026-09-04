using UnityEngine;

public readonly struct DamageInfo
{
    public float Amount { get; }

    public Vector3 HitPoint { get; }

    public Vector3 HitNormal { get; }

    public GameObject Instigator { get; }

    public WeaponData WeaponData { get; }


    public DamageInfo(
        float amount,
        Vector3 hitPoint,
        Vector3 hitNormal,
        GameObject instigator,
        WeaponData weaponData)
    {
        Amount =
            amount;

        HitPoint =
            hitPoint;

        HitNormal =
            hitNormal;

        Instigator =
            instigator;

        WeaponData =
            weaponData;
    }
}