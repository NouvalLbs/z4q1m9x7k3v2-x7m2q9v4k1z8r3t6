using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Data;

public static class WeaponData
{
    // Weapon slot per weaponId (0–46), mirrors ac_wSlot[]
    public static readonly int[] Slot =
    {
        0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 10,10,10,10,10,
        10, 8, 8, 8, 8, 8, 8, 2, 2, 2, 3, 3, 3, 4, 4,
        5, 5, 4, 6, 6, 7, 7, 7, 7, 8,12, 9, 9, 9,11,11,11
    };

    // Weapon model IDs for pickups, mirrors ac_wModel[]
    public static readonly int[] PickupModel =
    {
        0,331,333,334,335,336,337,338,339,341,321,322,323,324,325,
        326,342,343,344,345,345,345,346,347,348,349,350,351,352,353,355,356,
        372,357,358,359,360,361,362,363,364,365,366,367,368,369,371
    };

    // Default ammo from pickup, mirrors ac_pAmmo[]
    public static readonly int[] PickupAmmo =
    {
        1,1,1,1,1,1,1,1,1,1,1,1,1,
        1,1,1,8,8,8,8,4,4,30,10,10,15,
        10,10,60,60,80,80,60,20,10,4,3,
        100,500,5,1,500,500,36,0,0,1
    };

    // Min shooting range per bullet-weapon index, mirrors ac_wMinRange[]
    public static readonly float[] MinRange =
    {
        25f,25f,25f,30f,25f,35f,
        25f,35f,40f,40f,25f,55f,
        50f,50f,50f,4f,65f
    };

    // Ammo-based slots (slots where ammo is tracked)
    private static readonly HashSet<int> _ammoSlots = new() { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
    public static bool SlotHasAmmo(int slot) => _ammoSlots.Contains(slot);

    // Slots where all weapons share ammo pool (e.g. detonator)
    private static readonly HashSet<int> _sharedAmmoSlots = new() { 12 };
    public static bool SlotSharesAmmo(int slot) => _sharedAmmoSlots.Contains(slot);

    public static bool IsValid(int weaponId) => (uint)weaponId < (uint)Slot.Length;

    // Ammunation shop: {cost, ammo}, mirrors ac_AmmuNationInfo[][]
    public static readonly (int Cost, int Ammo)[] AmmuNation =
    {
        (200,30),(600,30),(1200,15),(600,15),(800,12),(1000,10),
        (500,60),(2000,90),(3500,120),(4500,150),(300,60)
    };
}