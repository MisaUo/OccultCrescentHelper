using System.Collections.Generic;
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
public class OCHIllegalCommand(Plugin plugin) : OcelotCommand
{
    public override string command
    {
        get => "/bocchiillegal";
    }

    public override string description
    {
        get => @"
Manage och automator/illegal mode.
 - /bocchiillegal (Toggles the automator lens window)
 - /bocchiillegal on (Enables illegal mode (Automation))
 - /bocchiillegal off (Disables illegal mode (Automation))
 - /bocchiillegal toggle (Toggles illegal mode (Automation))
--------------------------------
".Trim();
    }

    public override IReadOnlyList<string> aliases
    {
        get => ["/ochillegal", "/bocchillegal"];
    }

    public override IReadOnlyList<string> validArguments
    {
        get => ["on", "off", "toggle"];
    }

    public override void Command(string command, string arguments)
    {
        if (arguments.Trim() == "")
        {
            plugin.Windows.GetWindow<AutomatorWindow>()?.Toggle();
            return;
        }

        if (!plugin.Modules.TryGetModule<AutomatorModule>(out var automator) || automator == null)
        {
            return;
        }

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
    }

    private unsafe void FlagActiveCe(AgentMap* map)
    {
        if (!plugin.Modules.TryGetModule<CriticalEncountersModule>(out var source) || source == null)
        {
            return;
        }

        foreach (var encounter in source.CriticalEncounters.Values)
        {
            if (encounter.EventType >= 4 || encounter.State != DynamicEventState.Register)
            {
                continue;
            }

            map->SetFlagMapMarker(Svc.ClientState.TerritoryType, Svc.ClientState.MapId, encounter.MapMarker.Position);
            return;
        }
    }

    private unsafe void FlagActiveFate(AgentMap* map, bool ignorePots)
    {
        if (!plugin.Modules.TryGetModule<FatesModule>(out var source) || source == null)
        {
            return;
        }

        foreach (var fate in source.fates.Values)
        {
            if (ignorePots && fate.Data.Note == MonsterNote.PersistentPots)
            {
                continue;
            }

            map->SetFlagMapMarker(Svc.ClientState.TerritoryType, Svc.ClientState.MapId, fate.StartPosition);
            return;
        }
    }
}
