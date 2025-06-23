using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
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

    private bool finalDestination = false;

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

            if (!finalDestination && IsCloseToZone())
            {
                // Get all players in the zone
                var playersInZone = Svc.Objects
                    .Where(o => o.ObjectKind == ObjectKind.Player)
                    .Where(o => Vector3.Distance(o.Position, GetPosition()) <= 30f)
                    .ToList();

                if (playersInZone.Count > 0)
                {
                    var minX = playersInZone.Min(p => p.Position.X);
                    var maxX = playersInZone.Max(p => p.Position.X);
                    var minY = playersInZone.Min(p => p.Position.Z); // Y in 2D is Z in FFXIV
                    var maxY = playersInZone.Max(p => p.Position.Z);

                    // Choose a random point within the bounding box of players
                    var rand = new Random();
                    var randX = (float)(minX + rand.NextDouble() * (maxX - minX));
                    var randY = (float)(minY + rand.NextDouble() * (maxY - minY));
                    var randomPoint = new Vector3(randX, GetPosition().Y, randY);

                    module.Debug($"Pathfinding to random point: {randomPoint} (MinX: {minX}, MaxX: {maxX}, MinY: {minY}, MaxY: {maxY})");

                    vnav.PathfindAndMoveTo(randomPoint, false);
                    finalDestination = true;
                }
            }

            if (!finalDestination && IsInZone())
            {
                if (vnav.IsRunning())
                {
                    vnav.Stop();
                }

                return true;
            }

            var critical = module.GetModule<CriticalEncountersModule>();
            var encounter = critical.criticalEncounters[data.id];

            if (encounter.State != DynamicEventState.Register)
            {
                throw new Exception("This event started without you");
            }

            if (finalDestination)
            {
                return !vnav.IsRunning();
            }

            if (!vnav.IsRunning())
            {
                throw new VnavmeshStoppedException();
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

    private bool IsCloseToZone(float radius = 50f)
    {
        return Player.DistanceTo(GetPosition()) <= radius;
    }

}
