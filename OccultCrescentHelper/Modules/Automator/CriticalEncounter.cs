using System;
using System.Numerics;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using OccultCrescentHelper.Data;
using OccultCrescentHelper.Modules.CriticalEncounters;
using OccultCrescentHelper.Modules.StateManager;
using Ocelot.Chain;
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

        handlers.Add(ActivityState.WaitingToStartCriticalEncounter, GetWaitingToStartCriticalEncounterChain);
    }

    protected override TaskManagerTask GetPathfindingWatcher(StateManagerModule states, VNavmesh vnav)
    {
          return new(() => {
            if (!IsValid())
            {
                throw new Exception("Activity is no longer valid.");
            }

            if (IsInZone())
            {
                if (vnav.IsRunning())
                {
                    vnav.Stop();
                }

                return true;
            }

            if (!vnav.IsRunning())
            {
                throw new VnavmeshStoppedException();
            }

            var critical = module.GetModule<CriticalEncountersModule>();
            var encounter = critical.criticalEncounters[data.id];

            if (encounter.State != DynamicEventState.Register)
            {
                throw new Exception("This event started without you");
            }

            return false;
        }, new() { TimeLimitMS = 180000, ShowError = false });
    }


    public unsafe Func<Chain> GetWaitingToStartCriticalEncounterChain(StateManagerModule states)
    {
        return () => {
            return Chain.Create("Illegal:WaitingToStartCriticalEncounter")
                .Then(new TaskManagerTask(() => {
                    if (!IsValid())
                        throw new Exception("The critical encounter appears to have started without you.");

                    var critical = module.GetModule<CriticalEncountersModule>();
                    var encounter = critical.criticalEncounters[data.id];

                    if (encounter.State == DynamicEventState.Battle &&
                        states.GetState() != State.InCriticalEncounter)
                    {
                        throw new Exception("The critical encounter appears to have started without you.");
                    }

                    if (!vnav.IsRunning() && states.GetState() == State.InCombat)
                    {
                        // Unmount if we're in combat, and activate our AI provider
                        if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.Mounted])
                        {
                            ActionManager.Instance()->UseAction(
                                ActionType.Mount,
                                module.plugin.config.MountConfig.Mount
                            );
                        }

                        if (module.config.ShouldToggleAiProvider)
                        {
                            module.config.AiProvider.On();
                        }
                    }

                    return states.GetState() == State.InCriticalEncounter;
                },
                new() {
                    TimeLimitMS = 180000
                }))
                .Then(_ => state = ActivityState.Participating);
        };
    }

    public override bool IsValid()
    {
        if (encounter.State == DynamicEventState.Register)
        {
            return true;
        }

        if (encounter.State == DynamicEventState.Warmup)
        {
            return Player.DistanceTo(GetPosition()) <= 30f;
        }

        if (encounter.State == DynamicEventState.Battle)
        {
            return Player.Status.Has(PlayerStatus.HoofingIt);
        }

        return true;
    }

    public override Vector3 GetPosition() => encounter.MapMarker.Position;
}
