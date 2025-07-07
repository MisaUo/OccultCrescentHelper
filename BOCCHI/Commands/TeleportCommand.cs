using System.Collections.Generic;
using System.Linq;
using BOCCHI.Chains;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.Buff;
using BOCCHI.Modules.CriticalEncounters;
using BOCCHI.Modules.Fates;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Commands;
using Ocelot.IPC;
using Ocelot.Modules;

namespace BOCCHI.Commands;

[OcelotCommand]
public class TeleportCommand(Plugin plugin) : OcelotCommand
{
    public override string command
    {
        get => "/bocchitp";
    }

    public override string description
    {
        get => "";
    }


    public override void Command(string command, string arguments)
    {
        if (ZoneHelper.GetNearbyAethernetShards().Count <= 0)
        {
            Svc.Chat.Print("You are not near a aethernet shards.");
            return;
        }

        var lifestream = plugin.ipc.GetProvider<Lifestream>();
        if (!lifestream.IsReady() || lifestream.IsBusy())
        {
            Svc.Chat.Print("Lifestream is busy");
            return;
        }

        Aethernet? shard = null;
        if (arguments.Length <= 0)
        {
            shard ??= GetCriticalEncounterAethernet();
            shard ??= GetFateAethernet();
            shard ??= GetPotFateAethernet();
        }
        else
        {
            switch (arguments)
            {
                case "fate":
                    shard = GetFateAethernet();
                    break;
                case "ce":
                    shard = GetCriticalEncounterAethernet();
                    break;
                case "pot":
                    shard = GetPotFateAethernet();
                    break;
            }
        }

        if (shard == null)
        {
            Svc.Chat.Print("No aethernet shard found");
            return;
        }

        if (ZoneHelper.IsNearAethernetShard((Aethernet)shard))
        {
            Svc.Chat.Print("You are already at the closest shard");
            return;
        }

        Plugin.Chain.Submit(ChainHelper.TeleportChain((Aethernet)shard));
    }

    private Aethernet? GetFateAethernet()
    {
        var source = plugin.modules.GetModule<FatesModule>();
        foreach (var fate in source.fates.Values)
        {
            if (!EventData.Fates.TryGetValue(fate.FateId, out var data))
            {
                continue;
            }

            if (data.notes == MonsterNote.PersistentPots)
            {
                continue;
            }

            return data.aethernet ?? ZoneHelper.GetClosestAethernetShard(data.start ?? fate.Position);
        }

        return null;
    }

    private Aethernet? GetPotFateAethernet()
    {
        var source = plugin.modules.GetModule<FatesModule>();
        foreach (var fate in source.fates.Values)
        {
            if (!EventData.Fates.TryGetValue(fate.FateId, out var data))
            {
                continue;
            }

            if (data.notes != MonsterNote.PersistentPots)
            {
                continue;
            }

            return data.aethernet ?? ZoneHelper.GetClosestAethernetShard(data.start ?? fate.Position);
        }

        return null;
    }

    private Aethernet? GetCriticalEncounterAethernet()
    {
        var source = plugin.modules.GetModule<CriticalEncountersModule>();
        foreach (var encounter in source.criticalEncounters.Values)
        {
            if (encounter.EventType >= 4 || encounter.State != DynamicEventState.Register)
            {
                continue;
            }

            if (!EventData.CriticalEncounters.TryGetValue(encounter.DynamicEventId, out var data))
            {
                continue;
            }

            return data.aethernet ?? ZoneHelper.GetClosestAethernetShard(data.start ?? encounter.MapMarker.Position);
        }

        return null;
    }
}
