namespace ProjectSMP.Plugins.Anticheat.Data;

public static class TuningData
{
    // Component prices, mirrors ac_cPrice[]
    public static readonly int[] ComponentPrice =
    {
        400,550,200,250,100,150,80,500,500,200,1000,220,250,100,400,
        500,200,500,350,300,250,200,150,350,50,1000,480,480,770,680,370,
        370,170,120,790,150,500,690,190,390,500,390,1000,500,500,510,710,
        670,530,810,620,670,530,130,210,230,520,430,620,720,530,180,550,430,
        830,850,750,250,200,550,450,550,450,1100,1030,980,1560,1620,1200,
        1030,900,1230,820,1560,1350,770,100,1500,150,650,450,100,750,
        350,450,350,1000,620,1140,1000,940,780,830,3250,1610,1540,780,780,780,
        1610,1540,0,0,3340,3250,2130,2050,2040,780,940,780,940,780,860,
        780,1120,3340,3250,3340,1650,3380,3290,1590,830,800,1500,1000,800,
        580,470,870,980,150,150,100,100,490,600,890,1000,1090,840,910,
        1200,1030,1030,920,930,550,1050,1050,950,650,450,550,850,950,
        850,950,970,880,990,900,950,1000,900,1000,900,2050,2150,2130,
        2050,2130,2040,2150,2040,2095,2175,2080,2200,1200,1040,940,1100
    };

    // Valid mod component IDs per vehicle, packed bitmask, mirrors ac_vMods[]
    // Each vehicle uses 3 uints (96 bits) = 96 possible component slots
    private static readonly uint[] _vModsPacked =
    {
        0x033C2700,0x00000000,0x00FFFE00,0x00000007,0x00000000,0x00000000,
        0x021A27FA,0x00000000,0x00FFFE00,0x00000007,0x0003C000,0x00000000,
        0x02000700,0x00000000,0x00FFFE00,0x00000007,0x00000000,0x00000000,
        0x02000700,0x00000000,0x00FFFE00,0x00000007,0x00000000,0x00000000,
        0x023B2785,0x00000000,0x00FFFE00,0x00000007,0x00000000,0x00000000,
        0x02BC4703,0x00000000,0x00FFFE00,0x00000007,0x00000000,0x00000000,
        0x02000700,0x00000000,0x00FFFE00,0x00000007,0x00000000,0x00000000,
        0x02000700,0x00000000,0x00FFFE00,0x00000007,0x00000000,0x00000000,
        0x02000700,0x00000000,0x00FFFE00,0x00000007,0x00000000,0x00000000,
        0x02000700,0x00000000,0x00FFFE00,0x00000007,0x00000000,0x00000000,
        0x03BA278A,0x00000000,0x00FFFE00,0x00000007,0x00000000,0x00000000
    };

    public static bool IsValidComponent(int vehicleModel, int componentId)
    {
        int modelIdx = vehicleModel - 400;
        if ((uint)modelIdx >= 212u) return false;
        int compIdx = componentId - 1000;
        if ((uint)compIdx >= 96u) return false;

        int wordBase = modelIdx * 3;
        int wordOff = compIdx / 32;
        int bit = compIdx % 32;
        int packedIdx = wordBase + wordOff;
        if ((uint)packedIdx >= (uint)_vModsPacked.Length) return false;

        return (_vModsPacked[packedIdx] & (1u << bit)) != 0;
    }

    public static int GetComponentPrice(int componentId)
    {
        int idx = componentId - 1000;
        return (uint)idx < (uint)ComponentPrice.Length ? ComponentPrice[idx] : 0;
    }

    // Vending machine positions, mirrors ac_vMachines[]
    public static readonly (float X, float Y, float Z)[] VendingMachines =
    {
        (-862.8281f,1536.6094f,21.9844f),(2271.7266f,-76.4609f,25.9609f),
        (1277.8359f,372.5156f,18.9531f),(662.4297f,-552.1641f,15.7109f),
        (201.0156f,-107.6172f,0.8984f),(-253.7422f,2597.9531f,62.2422f),
        (-253.7422f,2599.7578f,62.2422f),(-76.0313f,1227.9922f,19.1250f),
        (-14.7031f,1175.3594f,18.9531f),(-1455.1172f,2591.6641f,55.2344f),
        (2352.1797f,-1357.1563f,23.7734f),(2325.9766f,-1645.1328f,14.2109f),
        (2139.5156f,-1161.4844f,23.3594f),(2153.2344f,-1016.1484f,62.2344f),
        (1928.7344f,-1772.4453f,12.9453f),(1154.7266f,-1460.8906f,15.1563f),
        (2480.8594f,-1959.2734f,12.9609f),(2060.1172f,-1897.6406f,12.9297f),
        (1729.7891f,-1943.0469f,12.9453f),(1634.1094f,-2237.5313f,12.8906f),
        (1789.2109f,-1369.2656f,15.1641f),(-2229.1875f,286.4141f,34.7031f),
        (2319.9922f,2532.8516f,10.2188f),(2845.7266f,1295.0469f,10.7891f),
        (2503.1406f,1243.6953f,10.2188f),(2647.6953f,1129.6641f,10.2188f),
        (-2420.2188f,984.5781f,44.2969f),(-2420.1797f,985.9453f,44.2969f),
        (2085.7734f,2071.3594f,10.4531f),(1398.8438f,2222.6094f,10.4219f),
        (1659.4609f,1722.8594f,10.2188f),(1520.1484f,1055.2656f,10.0f),
        (-1980.7891f,142.6641f,27.0703f),(-2118.9688f,-423.6484f,34.7266f)
    };

    public static bool IsNearVendingMachine(float x, float y, float z, float range = 2f)
    {
        float r2 = range * range;
        foreach (var (mx, my, mz) in VendingMachines)
            if ((x - mx) * (x - mx) + (y - my) * (y - my) + (z - mz) * (z - mz) < r2) return true;
        return false;
    }
}