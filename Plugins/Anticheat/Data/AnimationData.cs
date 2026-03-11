using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Data;

public static class AnimationData
{
    private static readonly HashSet<string> _validLibraries = new()
    {
        "AIRPORT", "Attractors", "BAR", "BASEBALL", "BD_FIRE", "benchpress",
        "BF_injection", "BIKED", "BIKEH", "BIKELEAP", "BIKES", "BIKEV",
        "BIKE_DBZ", "BMX", "BOMBER", "BOX", "BSKTBALL", "BUDDY", "BUS",
        "CAMERA", "CAR", "CARRY", "CAR_CHAT", "CASINO", "CHAINSAW",
        "CHOPPA", "CLOTHES", "COACH", "COLT45", "COP_AMBIENT", "COP_DVBYZ",
        "CRACK", "CRIB", "DAM_JUMP", "DANCING", "DEALER", "DILDO",
        "DODGE", "DOZER", "DRIVEBYS", "FAT", "FIGHT_B", "FIGHT_C",
        "FIGHT_D", "FIGHT_E", "FINALE", "FINALE2", "FLAME", "Flowers",
        "FOOD", "Freeweights", "GANGS", "GHANDS", "GHETTO_DB", "goggles",
        "GRAFFITI", "GRAVEYARD", "GRENADE", "GYMNASIUM", "HAIRCUTS",
        "HEIST9", "INT_HOUSE", "INT_OFFICE", "INT_SHOP", "JST_BUISNESS",
        "KART", "KISSING", "KNIFE", "LAPDAN1", "LAPDAN2", "LAPDAN3",
        "LOWRIDER", "MD_CHASE", "MD_END", "MEDIC", "MISC", "MTB",
        "MUSCULAR", "NEVADA", "ON_LOOKERS", "OTB", "PARACHUTE", "PARK",
        "PAULNMAC", "ped", "PLAYER_DVBYS", "PLAYIDLES", "POLICE", "POOL",
        "POOR", "PYTHON", "QUAD", "QUAD_DBZ", "RAPPING", "RIFLE",
        "RIOT", "ROB_BANK", "ROCKET", "RUSTLER", "RYDER", "SCRATCHING",
        "SHAMAL", "SHOP", "SHOTGUN", "SILENCED", "SKATE", "SMOKING",
        "SNIPER", "SPRAYCAN", "STRIP", "SUNBATHE", "SWAT", "SWEET",
        "SWIM", "SWORD", "TANK", "TATTOOS", "TEC", "TRAIN", "TRUCK",
        "UZI", "VAN", "VENDING", "VORTEX", "WAYFARER", "WEAPONS",
        "WUZI", "SNM", "BLOWJOBZ", "SEX", "BOMBER", "RAPPING", "SHOP",
        "BEACH", "SMOKING", "FOOD", "ON_LOOKERS"
    };

    private static readonly Dictionary<string, int> _libraryAnimCounts = new()
    {
        {"AIRPORT", 6}, {"Attractors", 17}, {"BAR", 12}, {"BASEBALL", 9},
        {"BD_FIRE", 7}, {"benchpress", 5}, {"BF_injection", 13}, {"BIKED", 11},
        {"BIKEH", 11}, {"BIKELEAP", 4}, {"BIKES", 11}, {"BIKEV", 11},
        {"BIKE_DBZ", 4}, {"BMX", 11}, {"BOMBER", 3}, {"BOX", 11},
        {"BSKTBALL", 13}, {"BUDDY", 4}, {"BUS", 3}, {"CAMERA", 4},
        {"CAR", 28}, {"CARRY", 9}, {"CAR_CHAT", 7}, {"CASINO", 19},
        {"CHAINSAW", 5}, {"CHOPPA", 4}, {"CLOTHES", 5}, {"COACH", 3},
        {"COLT45", 7}, {"COP_AMBIENT", 3}, {"COP_DVBYZ", 4}, {"CRACK", 3},
        {"CRIB", 11}, {"DAM_JUMP", 4}, {"DANCING", 18}, {"DEALER", 8},
        {"DILDO", 5}, {"DODGE", 3}, {"DOZER", 2}, {"DRIVEBYS", 6},
        {"FAT", 11}, {"FIGHT_B", 10}, {"FIGHT_C", 12}, {"FIGHT_D", 11},
        {"FIGHT_E", 10}, {"FINALE", 4}, {"FINALE2", 3}, {"FLAME", 2},
        {"Flowers", 4}, {"FOOD", 6}, {"Freeweights", 8}, {"GANGS", 52},
        {"GHANDS", 3}, {"GHETTO_DB", 2}, {"goggles", 2}, {"GRAFFITI", 2},
        {"GRAVEYARD", 3}, {"GRENADE", 3}, {"GYMNASIUM", 8}, {"HAIRCUTS", 6},
        {"HEIST9", 7}, {"INT_HOUSE", 9}, {"INT_OFFICE", 9}, {"INT_SHOP", 7},
        {"JST_BUISNESS", 8}, {"KART", 4}, {"KISSING", 14}, {"KNIFE", 5},
        {"LAPDAN1", 5}, {"LAPDAN2", 5}, {"LAPDAN3", 5}, {"LOWRIDER", 10},
        {"MD_CHASE", 6}, {"MD_END", 4}, {"MEDIC", 2}, {"MISC", 13},
        {"MTB", 11}, {"MUSCULAR", 11}, {"NEVADA", 2}, {"ON_LOOKERS", 11},
        {"OTB", 3}, {"PARACHUTE", 5}, {"PARK", 4}, {"PAULNMAC", 7},
        {"ped", 107}, {"PLAYER_DVBYS", 4}, {"PLAYIDLES", 3}, {"POLICE", 9},
        {"POOL", 6}, {"POOR", 11}, {"PYTHON", 3}, {"QUAD", 4},
        {"QUAD_DBZ", 4}, {"RAPPING", 4}, {"RIFLE", 3}, {"RIOT", 8},
        {"ROB_BANK", 12}, {"ROCKET", 3}, {"RUSTLER", 2}, {"RYDER", 5},
        {"SCRATCHING", 3}, {"SHAMAL", 2}, {"SHOP", 7}, {"SHOTGUN", 3},
        {"SILENCED", 3}, {"SKATE", 8}, {"SMOKING", 3}, {"SNIPER", 3},
        {"SPRAYCAN", 2}, {"STRIP", 18}, {"SUNBATHE", 3}, {"SWAT", 3},
        {"SWEET", 8}, {"SWIM", 8}, {"SWORD", 5}, {"TANK", 2},
        {"TATTOOS", 5}, {"TEC", 3}, {"TRAIN", 3}, {"TRUCK", 3},
        {"UZI", 3}, {"VAN", 2}, {"VENDING", 4}, {"VORTEX", 2},
        {"WAYFARER", 2}, {"WEAPONS", 15}, {"WUZI", 13}
    };

    private static readonly HashSet<string> _dangerousAnimations = new()
    {
        "PARACHUTE:FALL_skyDive",
        "PARACHUTE:FALL_skyDive_DIE",
        "SWIM:Swim_Dive_Under",
        "DAM_JUMP:SF_JumpWall",
        "DAM_JUMP:DAM_Dive_Loop",
        "KNIFE:KILL_Knife_Ped_Die",
        "KNIFE:KILL_Knife_Player",
        "SWEET:Sweet_injuredloop",
        "MISC:Scratchballs_01"
    };

    private static readonly HashSet<string> _freezeAnimations = new()
    {
        "ped:IDLE_STANCE",
        "ped:WALK_civi",
        "WUZI:CS_Dead_Guy",
        "SWEET:Sweet_injuredloop"
    };

    public static bool IsValidLibrary(string library)
        => _validLibraries.Contains(library);

    public static bool IsValidAnimationCount(string library, int maxExpected)
    {
        if (!_libraryAnimCounts.TryGetValue(library, out int maxCount))
            return true;
        return maxExpected <= maxCount + 5;
    }

    public static bool IsDangerousAnimation(string library, string animName)
        => _dangerousAnimations.Contains($"{library}:{animName}");

    public static bool IsFreezeAnimation(string library, string animName)
        => _freezeAnimations.Contains($"{library}:{animName}");

    public static bool IsIncompatibleWithVehicle(string library)
        => library is "PARACHUTE" or "SWIM" or "SUNBATHE" or "BEACH";
}