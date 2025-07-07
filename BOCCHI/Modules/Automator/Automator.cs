using System.Linq;
using BOCCHI.Chains;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.CriticalEncounters;
using BOCCHI.Modules.Fates;
using BOCCHI.Modules.StateManager;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Chain;
using Ocelot.IPC;

namespace BOCCHI.Modules.Automator;

public class Automator
{
    private bool IsChainActive
    {
        get => ChainManager.Active().Count > 0;
    }

    public Activity? activity { get; private set; } = null;

    private int idleTime = 0;

    private bool firstTick = true;

    public void Tick(AutomatorModule module, IFramework framework)
    {
        if (firstTick)
        {
            firstTick = false;
            return;
        }

        if (!module.TryGetIPCProvider<VNavmesh>(out var vnav) || vnav == null)
        {
            return;
        }

        if (!module.TryGetIPCProvider<Lifestream>(out var lifestream) || lifestream == null)
        {
            return;
        }

        var states = module.GetModule<StateManagerModule>();
        if (activity == null)
        {
            if (states.GetState() == State.InCombat)
            {
                return;
            }

            if (states.GetState() == State.InCriticalEncounter)
            {
                var critical = module.GetModule<CriticalEncountersModule>();
                var encounter = critical.criticalEncounters.Values.Where((ev) => ev.State != DynamicEventState.Inactive).Last();
                var data = EventData.CriticalEncounters[encounter.DynamicEventId];
                activity = new CriticalEncounter(data, lifestream, vnav, module, critical);

                if (activity != null)
                {
                    module.Debug($"Resuming running activity: {activity.data.Name}");
                }

                return;
            }

            if (states.GetState() == State.InFate)
            {
                activity ??= FindFate(module, lifestream, vnav);

                if (activity != null)
                {
                    module.Debug($"Resuming running activity: {activity.data.Name}");
                }

                return;
            }
        }

        if (activity != null && !activity.IsValid())
        {
            Plugin.Chain.Abort();
            vnav.Stop();
            activity = null;
        }

        if (IsChainActive)
        {
            return;
        }

        if (activity != null)
        {
            if (activity.state == ActivityState.Done)
            {
                activity = null;
                return;
            }

            var chain = activity.GetChain(states);
            if (chain == null)
            {
                return;
            }

            Plugin.Chain.Submit(chain);
            return;
        }

        if (!module.config.ShouldDoFates && !module.config.ShouldDoCriticalEncounters)
        {
            return;
        }

        // Try and get the next activity
        activity ??= module.config.ShouldDoCriticalEncounters ? FindCriticalEncounter(module, lifestream, vnav) : null;
        activity ??= module.config.ShouldDoFates ? FindFate(module, lifestream, vnav) : null;
        if (activity != null)
        {
            Svc.Log.Info($"Selected activity: {activity.data.Name}");
            return;
        }

        var closest = AethernetData.GetClosestToPlayer();
        if (closest.DistanceToPlayer() <= 4.5f)
        {
            return;
        }

        idleTime += framework.UpdateDelta.Milliseconds;
        if (idleTime > 3000)
        {
            idleTime = 0;

            Plugin.Chain.Submit(ChainHelper.ReturnChain());
        }
    }

    public Activity? FindCriticalEncounter(AutomatorModule module, Lifestream lifestream, VNavmesh vnav)
    {
        if (!module.TryGetModule<CriticalEncountersModule>(out var source) || source == null)
        {
            return null;
        }

        foreach (var encounter in source.criticalEncounters.Values)
        {
            if (!module.config.CriticalEncountersMap.TryGetValue(encounter.DynamicEventId, out var enabled) || !enabled)
            {
                continue;
            }

            if (encounter.State != DynamicEventState.Register)
            {
                continue;
            }

            if (!EventData.CriticalEncounters.TryGetValue(encounter.DynamicEventId, out var data))
            {
                continue;
            }

            return new CriticalEncounter(data, lifestream, vnav, module, source);
        }

        return null;
    }

    public Activity? FindFate(AutomatorModule module, Lifestream lifestream, VNavmesh vnav)
    {
        if (!module.TryGetModule<FatesModule>(out var source) || source == null)
        {
            return null;
        }

        foreach (var fate in source.fates.Values)
        {
            if (
                fate == null
                || !module.config.FatesMap[fate.FateId] == true
                || !EventData.Fates.TryGetValue(fate.FateId, out var data)
            )
            {
                continue;
            }

            return new Fate(data, lifestream, vnav, module, fate);
        }

        return null;
    }

    public void Refresh()
    {
        activity = null;
        idleTime = 0;
        firstTick = true;
    }
}
