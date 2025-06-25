using System.Collections.Generic;
using System.Numerics;
using ECommons.DalamudServices;

namespace OccultCrescentHelper.Data;

public static class ZoneData
{
    public const uint SOUTHHORN = 1252;

    public readonly static List<uint> zoneIds =
    [
        SOUTHHORN,
    ];

    // This can and should be filled using layout files or excel data
    public readonly static Dictionary<uint, Vector3> aetherytes = new()
    {
        { SOUTHHORN, new Vector3(830.75f, 72.98f, -695.98f) },
    };

    public readonly static Dictionary<uint, Vector3> startingLocations = new()
    {
        { SOUTHHORN, new Vector3(850.33f, 72.99f, -704.07f) },
    };

    // Zone functions
    public static bool IsInSouthHorn()
    {
        return Svc.ClientState.TerritoryType == SOUTHHORN;
    }

    public static bool IsInOccultCrescent()
    {
        return Svc.ClientState.LocalPlayer != null && IsInSouthHorn();
    }

    // Tower functions
    public static bool IsInForkedTowerBlood()
    {
        var player = Svc.ClientState.LocalPlayer;
        if (player == null)
        {
            return false;
        }

        return player.StatusList.HasAny(
            PlayerStatus.DutiesAsAssigned,
            PlayerStatus.ResurrectionDenied,
            PlayerStatus.ResurrectionRestricted
        );
    }

    public static bool IsInForkedTower()
    {
        return IsInForkedTowerBlood();
    }
}
