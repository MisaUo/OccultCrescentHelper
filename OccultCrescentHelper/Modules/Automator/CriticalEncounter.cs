
using System.Numerics;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using OccultCrescentHelper.Data;
using OccultCrescentHelper.Modules.CriticalEncounters;
using Ocelot.IPC;

namespace OccultCrescentHelper.Modules.Automator;

public class CriticalEncounter : Activity
{
    private CriticalEncountersModule critical;

    private DynamicEvent encounter => critical.criticalEncounters[data.id];

    public CriticalEncounter(EventData data, Lifestream lifestream, VNavmesh vnav, AutomatorModule module, CriticalEncountersModule critical)
        : base(data, lifestream, vnav, module)
    {
        this.critical = critical;
    }

    public override bool IsValid()
    {
        if (encounter.State == DynamicEventState.Register)
        {
            return true;
        }

        if (encounter.State == DynamicEventState.Warmup || encounter.State == DynamicEventState.Battle)
        {
            return Player.Status.Has(PlayerStatus.HoofingIt);
        }

        return true;
    }

    public override Vector3 GetPosition() => encounter.MapMarker.Position;
}
