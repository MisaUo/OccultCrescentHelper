using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using OccultCrescentHelper.Chains;
using OccultCrescentHelper.Data;
using OccultCrescentHelper.Enums;
using OccultCrescentHelper.Modules.CriticalEncounters;
using OccultCrescentHelper.Modules.Fates;
using OccultCrescentHelper.Modules.StateManager;
using Ocelot.Chain;
using Ocelot.IPC;

namespace OccultCrescentHelper.Modules.Automator;

public abstract class Activity
{
    public readonly EventData data;

    protected Lifestream lifestream;

    protected VNavmesh vnav;

    protected AutomatorModule module;

    public ActivityState state = ActivityState.Idle;

    protected Dictionary<ActivityState, Func<StateManagerModule, Func<Chain>?>> handlers;

    public Activity(EventData data, Lifestream lifestream, VNavmesh vnav, AutomatorModule module)
    {
        this.data = data;
        this.lifestream = lifestream;
        this.vnav = vnav;
        this.module = module;

        handlers = new() {
            { ActivityState.Idle, GetIdleChain },
            { ActivityState.Pathfinding, GetPathfindingChain },
            { ActivityState.Participating, GetParticipatingChain },
            { ActivityState.Done, GetDoneChain },
        };

        var states = module.GetModule<StateManagerModule>();
        if (states.GetState() == State.InFate || states.GetState() == State.InCriticalEncounter)
        {
            this.state = ActivityState.Participating;
        }
    }


    public unsafe Func<Chain>? GetChain(StateManagerModule states)
    {
        if (!IsValid())
        {
            return null;
        }

        return handlers[state](states);
    }

    public Func<Chain> GetIdleChain(StateManagerModule states)
    {
        return () => {
            return Chain.Create("Illegal:Idle")
                .ConditionalThen(_ => module.config.ShouldToggleAiProvider && !Svc.Condition[ConditionFlag.InCombat], _ => module.config.AiProvider.Off())
                .Then(_ => vnav.Stop())
                .Then(_ => state = ActivityState.Pathfinding);
        };
    }

    public Func<Chain> GetPathfindingChain(StateManagerModule states)
    {
        return () => {
            var yes = module.GetIPCProvider<YesAlready>();

            var playerShard = AethernetData.All().OrderBy((data) => Vector3.Distance(Player.Position, data.position)).First();
            var activityShard = GetAethernetData();

            bool isFate = data.type == EventType.Fate;
            var navType = SmartNavigation.Decide(Player.Position, GetPosition(), activityShard);

            module.Debug("Selected navigation type: " + navType.ToString());

            var chain = Chain.Create("Illegal:Pathfinding")
                .ConditionalWait(_ => !isFate && module.config.ShouldDelayCriticalEncounters, Random.Shared.Next(10000, 15001));

            switch (navType)
            {
                case NavigationType.WalkToEvent:
                    chain
                        .ConditionalThen(_ => ShouldMountToPathfindTo(GetPosition()), ChainHelper.MountChain())
                        .Then(new PathfindingChain(vnav, GetPosition(), data, false));
                    break;

                case NavigationType.ReturnThenWalkToEvent:
                    chain
                        .Then(ChainHelper.ReturnChain())
                        .ConditionalThen(_ => ShouldMountToPathfindTo(GetPosition()), ChainHelper.MountChain())
                        .Then(new PathfindingChain(vnav, GetPosition(), data, false));
                    break;

                case NavigationType.ReturnThenTeleportToEventshard:
                    chain
                        .Then(ChainHelper.ReturnChain())
                        .Then(ChainHelper.TeleportChain(activityShard.aethernet))
                        .Debug("Waiting for lifestream to not be 'busy'")
                        .Then(new TaskManagerTask(() => !lifestream.IsBusy(), new() { TimeLimitMS = 30000 }))
                        .Then(new PathfindingChain(vnav, GetPosition(), data, false));
                    break;

                case NavigationType.WalkToClosestShardAndTeleportToEventShardThenWalkToEvent:
                    chain
                        .ConditionalThen(_ => ShouldMountToPathfindTo(GetPosition()), ChainHelper.MountChain())
                        .Then(ChainHelper.PathfindToAndWait(playerShard.position, AethernetData.DISTANCE))
                        .Then(ChainHelper.TeleportChain(activityShard.aethernet))
                        .Debug("Waiting for lifestream to not be 'busy'")
                        .Then(new TaskManagerTask(() => !lifestream.IsBusy(), new() { TimeLimitMS = 30000 }))
                        .Then(new PathfindingChain(vnav, GetPosition(), data, false));
                    break;
            }

            chain
                .Then(GetPathfindingWatcher(states, vnav))
                // Cringe
                .Then(_ => state = isFate ? ActivityState.Participating : ActivityState.WaitingToStartCriticalEncounter);

            return chain;
        };
    }


    protected Func<Chain>? GetParticipatingChain(StateManagerModule states)
    {
        return () => {
            return Chain.Create("Illegal:Participating")
                .ConditionalThen(_ => module.config.ShouldToggleAiProvider, _ => module.config.AiProvider.On())
                .Then(_ => vnav.Stop())
                .Then(new TaskManagerTask(() => {
                    if (module.config.ShouldForceTarget && EzThrottler.Throttle("Participating.ForceTarget", 100))
                    {
                        Svc.Targets.Target ??= module.config.ShouldForceTargetCentralEnemy ? GetCentralMostEnemy() : GetClosestEnemy();
                    }

                    return states.GetState() == State.Idle;
                }, new() { TimeLimitMS = int.MaxValue }))
                .Then(_ => state = ActivityState.Done);
        };
    }

    protected Func<Chain>? GetDoneChain(StateManagerModule states)
    {
        return null;
    }

    private List<IGameObject> GetEnemies()
    {
        return Svc.Objects
            .Where(o =>
                o != null &&
                o.ObjectKind == ObjectKind.BattleNpc &&
                IsActivityTarget(o) &&
                o.IsTargetable
            )
            .OrderBy(o => Vector3.Distance(o.Position, Player.Position))
            .ToList();
    }

    protected int GetEnemyCount()
    {
        return GetEnemies().Count();
    }

    protected IGameObject? GetClosestEnemy()
    {
        return GetEnemies().FirstOrDefault();
    }

    protected IGameObject? GetCentralMostEnemy()
    {
        var enemies = GetEnemies();

        if (enemies.Count == 0)
            return null;

        var centroid = new Vector3(
            enemies.Average(o => o.Position.X),
            enemies.Average(o => o.Position.Y),
            enemies.Average(o => o.Position.Z)
        );

        return enemies
            .OrderBy(o => Vector3.Distance(o.Position, centroid))
            .FirstOrDefault();
    }

    private unsafe bool IsActivityTarget(IGameObject? obj)
    {
        if (obj == null)
        {
            return false;
        }

        try
        {
            var battleChara = (BattleChara*)obj.Address;

            var id = battleChara->EventId.EntryId;
            var count = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.DynamicEvent>().Count();

            if (data.type == EventType.Fate)
            {
                return battleChara->FateId == data.id;
            }

            var isRelatedToCurrentEvent = battleChara->EventId.EntryId == Player.BattleChara->EventId.EntryId;

            return obj.SubKind == (byte)BattleNpcSubKind.Enemy && isRelatedToCurrentEvent;
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex.Message);
            return false;
        }
    }

    public AethernetData GetAethernetData()
        => data.aethernet?.GetData() ?? AethernetData.All().OrderBy((data) => Vector3.Distance(GetPosition(), data.position)).First();

    public bool IsInZone()
    {
        float radius = data.radius ?? 0f;
        if (radius == 0f)
        {
            if (data.type == EventType.Fate)
            {
                radius = module.GetModule<FatesModule>().fates[data.id].Radius;
            }
            else
            {
                radius = module.GetModule<CriticalEncountersModule>().criticalEncounters[data.id].Unknown4;
            }
        }

        return Vector3.Distance(Player.Position, GetPosition()) <= radius;
    }

    private bool ShouldMountToPathfindTo(Vector3 destination)
    {
        if (!module._config.TeleporterConfig.ShouldMount)
        {
            return false;
        }

        return Vector3.Distance(Player.Position, destination) <= 20f;
    }

    protected abstract TaskManagerTask GetPathfindingWatcher(StateManagerModule states, VNavmesh vnav);

    public abstract bool IsValid();

    public abstract Vector3 GetPosition();
}
