using System.Linq;
using Dalamud.Game.ClientState.Statuses;

namespace OccultCrescentHelper.Data;

public enum PlayerStatus : uint
{
    // Generic
    HoofingIt = 1778,
    // Zone Specific
    DutiesAsAssigned = 4228,
    EnduringFortitude = 4233,
    Fleetfooted = 4239,
    RomeosBallad = 4244,
    ResurrectionRestricted = 4262,
    ResurrectionDenied = 4263,
    PhantomFreelancer = 4242,
    PhantomKnight = 4358,
    PhantomBerserker = 4359,
    PhantomMonk = 4360,
    PhantomRanger = 4361,
    PhantomSamurai = 4362,
    PhantomBard = 4363,
    PhantomGeomancer = 4364,
    PhantomTimeMage = 4365,
    PhantomCannoneer = 4366,
    PhantomChemist = 4367,
    PhantomOracle = 4368,
    PhantomThief = 4369
}


public static class StatusListExtensions
{
    public static bool Has(this StatusList current, PlayerStatus status)
    {
        return current.HasAny(status);
    }

    public static bool HasAny(this StatusList current, params PlayerStatus[] statuses)
    {
        foreach (var status in statuses)
        {
            if (current.Any(s => s.StatusId == (uint)status))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasAll(this StatusList current, params PlayerStatus[] statuses)
    {
        foreach (var status in statuses)
        {
            if (!current.Any(s => s.StatusId == (uint)status))
            {
                return false;
            }
        }

        return true;
    }
}
