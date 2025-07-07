using System;
using BOCCHI.Data;
using BOCCHI.Modules.Fates;
using Dalamud.Game.ClientState.Fates;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace BOCCHI.Modules.CriticalEncounters;

public class TowerTimer : IDisposable
{
    private CriticalEncounterTracker tracker;

    private FatesModule fates;

    private DateTime LastForkedTowerEnd = DateTime.Now;

    private DateTime LastForkedTowerRegister = DateTime.Now;

    private TimeSpan ForkedTowerSpawnTimer = TimeSpan.FromMinutes(5);

    private TimeSpan ForkedTowerWarmUpTimer = TimeSpan.FromSeconds(330);

    private int FatesCompleted = 0;

    private int CriticalEncountersCompleted = 0;

    public TowerTimer(CriticalEncounterTracker tracker, FatesModule fates)
    {
        this.tracker = tracker;
        this.fates = fates;

        fates.tracker.OnFateDespawned += OnFateDespawned;
        tracker.OnInactiveState += OnCriticalEncounterDespawned;
        tracker.OnRegisterState += OnCriticalEncounterRegistered;
        Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
    }

    public TimeSpan GetTimeToForkedTowerSpawn(DynamicEventState state)
    {
        if (state != DynamicEventState.Inactive)
        {
            return TimeSpan.Zero;
        }

        var fateModifier = TimeSpan.FromMinutes(1) * FatesCompleted;
        var criticalModifier = TimeSpan.FromMinutes(5) * CriticalEncountersCompleted;

        var time = LastForkedTowerEnd + (ForkedTowerSpawnTimer - fateModifier - criticalModifier) - DateTime.Now;
        if (time < TimeSpan.Zero)
        {
            ForkedTowerSpawnTimer = TimeSpan.FromMinutes(60);
            time = LastForkedTowerEnd + (ForkedTowerSpawnTimer - fateModifier - criticalModifier) - DateTime.Now;
        }

        return time;
    }

    public TimeSpan GetTimeRemainingToRegister(DynamicEventState state)
    {
        if (state != DynamicEventState.Register && state != DynamicEventState.Warmup)
        {
            return TimeSpan.Zero;
        }

        return LastForkedTowerRegister + ForkedTowerWarmUpTimer - DateTime.Now;
    }

    public int GetcompletedFates()
    {
        return FatesCompleted;
    }

    public int GetCompletedCriticalEncounters()
    {
        return CriticalEncountersCompleted;
    }

    private void OnFateDespawned(IFate _)
    {
        FatesCompleted++;
    }

    private void OnCriticalEncounterDespawned(DynamicEvent ev)
    {
        CriticalEncountersCompleted++;

        if (ev.EventType < 4)
        {
            return;
        }

        LastForkedTowerEnd = DateTime.Now;
        LastForkedTowerRegister = DateTime.Now;

        FatesCompleted = 0;
        CriticalEncountersCompleted = 0;
        ForkedTowerSpawnTimer = TimeSpan.FromMinutes(60);
    }

    private void OnCriticalEncounterRegistered(DynamicEvent ev)
    {
        if (ev.EventType < 4)
        {
            return;
        }

        LastForkedTowerRegister = DateTime.Now;
    }

    private void OnTerritoryChanged(ushort _)
    {
        if (!ZoneData.IsInOccultCrescent())
        {
            return;
        }

        FatesCompleted = 0;
        CriticalEncountersCompleted = 0;

        LastForkedTowerEnd = DateTime.Now;
        LastForkedTowerRegister = DateTime.Now;
        ForkedTowerSpawnTimer = TimeSpan.FromMinutes(5);
    }

    public void Dispose()
    {
        fates.tracker.OnFateDespawned -= OnFateDespawned;
        tracker.OnInactiveState -= OnCriticalEncounterDespawned;
        tracker.OnRegisterState -= OnCriticalEncounterRegistered;
        Svc.ClientState.TerritoryChanged -= OnTerritoryChanged;
    }
}
