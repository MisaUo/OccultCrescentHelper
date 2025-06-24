using System.Collections.Generic;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.CriticalEncounters;
using BOCCHI.Modules.Fates;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Ocelot.Commands;
using Ocelot.Modules;

namespace BOCCHI.Commands;

[OcelotCommand]
public class OCHCmdCommand : OcelotCommand
{
    private readonly Plugin plugin;

    public OCHCmdCommand(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public override string command => "/bocchicmd";

    public override string description => @"
Utility command.
 - Flag commands clear active flag before trying to place a new one
   - /bocchicmd flag-active-ce (Place a flag marker on the current Critical Engagement)
   - /bocchicmd flag-active-fate (Place a flag marker on a current Fate)
   - /bocchicmd flag-active-non-pot-fate (Place a flag marker on a current fate that isn't a pot fate)
--------------------------------
".Trim();

    public override IReadOnlyList<string> aliases => ["/ochcmd"];

    public override IReadOnlyList<string> validArguments =>
        ["flag-active-ce", "flag-active-fate", "flag-active-non-pot-fate"];

    public override unsafe void Command(string command, string arguments)
    {
        Svc.Framework.RunOnTick(() => {
            var map = AgentMap.Instance();
            map->IsFlagMarkerSet = false;

            switch (arguments)
            {
                case "flag-active-ce": FlagActiveCe(map); break;
                case "flag-active-fate": FlagActiveFate(map, false); break;
                case "flag-active-non-pot-fate": FlagActiveFate(map, true); break;
            }
        });
    }

    private unsafe void FlagActiveCe(AgentMap* map)
    {
        if (!plugin.modules.TryGetModule<CriticalEncountersModule>(out var source) || source == null) return;

        foreach (var encounter in source.criticalEncounters.Values)
        {
            if (encounter.EventType >= 4 || encounter.State != DynamicEventState.Register) continue;

            map->SetFlagMapMarker(Svc.ClientState.TerritoryType, Svc.ClientState.MapId, encounter.MapMarker.Position);
            return;
        }
    }

    private unsafe void FlagActiveFate(AgentMap* map, bool ignorePots)
    {
        if (!plugin.modules.TryGetModule<FatesModule>(out var source) || source == null) return;

        foreach (var fate in source.fates.Values)
        {
            if (fate == null) continue;

            if (!EventData.Fates.TryGetValue(fate.FateId, out var data)) continue;

            if (ignorePots && data.notes == MonsterNote.PersistentPots) continue;

            map->SetFlagMapMarker(Svc.ClientState.TerritoryType, Svc.ClientState.MapId, data.start ?? fate.Position);
            return;
        }
    }
}
