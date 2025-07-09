using System;
using System.Collections.Generic;
using BOCCHI.Data;
using BOCCHI.Enums;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace BOCCHI.Modules.CriticalEncounters;

public class Alerter : IDisposable
{
    private readonly CriticalEncountersModule module;

    public Dictionary<Demiatma, Func<bool>> DemiatmaAlerts
    {
        get => new()
        {
            [Demiatma.Azurite] = () => module.config.AlertAzurite,
            [Demiatma.Verdigris] = () => module.config.AlertVerdigris,
            [Demiatma.Malachite] = () => module.config.AlertMalachite,
            [Demiatma.Realgar] = () => module.config.AlertRealgar,
            [Demiatma.CaputMortuum] = () => module.config.AlertCaputMortuum,
            [Demiatma.Orpiment] = () => module.config.AlertOrpiment,
        };
    }

    public Dictionary<SoulShard, Func<bool>> SoulShardAlerts
    {
        get => new()
        {
            [SoulShard.Oracle] = () => module.config.AlertOracle,
            [SoulShard.Berserker] = () => module.config.AlertBerserker,
            [SoulShard.Ranger] = () => module.config.AlertRanger,
        };
    }

    public Alerter(CriticalEncountersModule module)
    {
        this.module = module;

        this.module.tracker.OnRegisterState += OnCriticalEncounterSpawned;
        this.module.tracker.OnInactiveState += OnCriticalEncounterDepawned;
    }

    private void OnCriticalEncounterSpawned(DynamicEvent ev)
    {
        if (module.config.LogSpawn)
        {
            Svc.Chat.Print($"{ev.Name} has Spawned");
        }

        if (!ShouldAlertForCriticalEncounter(ev))
        {
            return;
        }

        UIGlobals.PlaySoundEffect(66);
    }

    private void OnCriticalEncounterDepawned(DynamicEvent ev)
    {
        if (module.config.LogSpawn)
        {
            Svc.Chat.Print($"{ev.Name} has Despawned");
        }

        if (!ShouldAlertForCriticalEncounter(ev))
        {
            return;
        }

        UIGlobals.PlaySoundEffect(68);
    }

    private bool ShouldAlertForCriticalEncounter(DynamicEvent ev)
    {
        if (module.config.AlertAll)
        {
            return true;
        }

        if (!EventData.CriticalEncounters.TryGetValue(ev.DynamicEventId, out var data))
        {
            return false;
        }

        if (data.demiatma != null)
        {
            var demiatma = (Demiatma)data.demiatma;
            if (DemiatmaAlerts.TryGetValue(demiatma, out var getter))
            {
                return getter();
            }
        }

        if (data.soulshard != null)
        {
            var soulshard = (SoulShard)data.soulshard;
            if (SoulShardAlerts.TryGetValue(soulshard, out var getter))
            {
                return getter();
            }
        }

        return false;
    }

    public void Dispose()
    {
        module.tracker.OnRegisterState -= OnCriticalEncounterSpawned;
        module.tracker.OnInactiveState -= OnCriticalEncounterDepawned;
    }
}
