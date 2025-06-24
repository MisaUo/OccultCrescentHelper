using System.Collections.Generic;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.Automator;
using BOCCHI.Modules.CriticalEncounters;
using BOCCHI.Modules.Fates;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Ocelot.Commands;
using Ocelot.Modules;

namespace BOCCHI.Commands;

[OcelotCommand]
public class OCHIllegalCommand : OcelotCommand
{
    private readonly Plugin plugin;

    public OCHIllegalCommand(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public override string command => "/bocchiillegal";

    public override string description => @"
Manage och automator/illegal mode.
 - /bocchiillegal (Toggles the automator lens window)
 - /bocchiillegal on (Enables illegal mode (Automation))
 - /bocchiillegal off (Disables illegal mode (Automation))
 - /bocchiillegal toggle (Toggles illegal mode (Automation))
--------------------------------
".Trim();

    public override IReadOnlyList<string> aliases => ["/ochillegal", "/bocchillegal"];

    public override IReadOnlyList<string> validArguments => ["on", "off", "toggle"];

    public override void Command(string command, string arguments)
    {
        if (arguments.Trim() == "")
        {
            plugin.windows.GetWindow<AutomatorWindow>()?.Toggle();
            return;
        }

        Svc.Framework.RunOnTick(() => {
            if (!plugin.modules.TryGetModule<AutomatorModule>(out var automator) || automator == null) return;

            switch (arguments)
            {
                case "on":
                    automator.EnableIllegalMode();
                    break;
                case "off":
                    automator.DisableIllegalMode();
                    break;
                case "toggle":
                    AutomatorModule.ToggleIllegalMode(plugin);
                    break;
            }

            plugin.Config.Save();
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
