﻿using BOCCHI.Enums;
using BOCCHI.Modules.CriticalEncounters;
using BOCCHI.Modules.Fates;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Ocelot.Commands;
using Ocelot.Modules;
using System.Collections.Generic;

namespace BOCCHI.Commands;

[OcelotCommand]
public class OCHCmdCommand(Plugin plugin) : OcelotCommand
{
    protected override string Command
    {
        get => "/bocchicmd";
    }

    protected override string Description
    {
        get => @"
实用命令。
   - 标志命令在尝试放置新标志之前将清除现有标志
   - /bocchicmd flag-active-ce （在当前CE地点放置标记）
   - /bocchicmd flag-active-fate （在当前FATE上放置标记）
   - /bocchicmd flag-active-non-pot-fate （在当前非POT FATE处放置标记）
--------------------------------
".Trim();
    }

    protected override IReadOnlyList<string> Aliases
    {
        get => ["/ochcmd"];
    }

    protected override IReadOnlyList<string> ValidArguments
    {
        get => ["flag-active-ce", "flag-active-fate", "flag-active-non-pot-fate"];
    }

    public override unsafe void Execute(string command, string arguments)
    {
        var map = AgentMap.Instance();
        map->IsFlagMarkerSet = false;

        switch (arguments)
        {
            case "flag-active-ce":
                FlagActiveCe(map);
                break;
            case "flag-active-fate":
                FlagActiveFate(map, false);
                break;
            case "flag-active-non-pot-fate":
                FlagActiveFate(map, true);
                break;
        }
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
