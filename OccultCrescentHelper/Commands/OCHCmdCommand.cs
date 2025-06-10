using System.Collections.Generic;
using Ocelot.Modules;
using Ocelot.Commands;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using OccultCrescentHelper.Modules.CriticalEncounters;
using OccultCrescentHelper.Modules.Fates;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using OccultCrescentHelper.Data;

namespace OccultCrescentHelper.Commands;

[OcelotCommand]
public class OCHCmdCommand : OcelotCommand
{
    public override string command => "/ochcmd";

    public override string description => @"
Utility command.
 - Flag commands clear active flag before trying to place a new one
   - /ochcmd flag-active-ce (Place a flag marker on the current Critical Engagement)
   - /ochcmd flag-active-fate (Place a flag marker on a current Fate)
   - /ochcmd flag-active-non-pot-fate (Place a flag marker on a current fate that isn't a pot fate)
--------------------------------
".Trim();

    public override IReadOnlyList<string> validArguments => ["flag-active-ce", "flag-active-fate", "flag-active-non-pot-fate"];

    private readonly Plugin plugin;

    public OCHCmdCommand(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public unsafe override void Command(string command, string arguments)
    {
        Svc.Framework.RunOnTick(() => {
            AgentMap* map = AgentMap.Instance();
            map->IsFlagMarkerSet = false;

            switch (arguments)
            {
                case "flag-active-ce": FlagActiveCe(map); break;
                case "flag-active-fate": FlagActiveFate(map, ignorePots: false); break;
                case "flag-active-non-pot-fate": FlagActiveFate(map, ignorePots: true); break;
            }
        });
    }

    private unsafe void FlagActiveCe(AgentMap* map)
    {
        if (!plugin.modules.TryGetModule<CriticalEncountersModule>(out var source) || source == null)
        {
            return;
        }

        uint index = 0;
        foreach (var encounter in source.criticalEncounters)
        {
            // Skip tower and non register encounters
            if (index == 0 || encounter.State != DynamicEventState.Register)
            {
                index++;
                continue;
            }

            map->SetFlagMapMarker(Svc.ClientState.TerritoryType, Svc.ClientState.MapId, encounter.MapMarker.Position);
            return;
        }
    }

    private unsafe void FlagActiveFate(AgentMap* map, bool ignorePots)
    {
        if (!plugin.modules.TryGetModule<FatesModule>(out var source) || source == null)
        {
            return;
        }

        foreach (var fate in source.fates.Values)
        {
            if (fate == null)
            {
                continue;
            }

            if (!EventData.Fates.TryGetValue(fate.FateId, out var data))
            {
                continue;
            }

            if (ignorePots && data.notes == Enums.MonsterNote.PersistentPots)
            {
                continue;
            }

            map->SetFlagMapMarker(Svc.ClientState.TerritoryType, Svc.ClientState.MapId, data.start ?? fate.Position);
            return;
        }

    }
}
