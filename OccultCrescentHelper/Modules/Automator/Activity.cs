using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using OccultCrescentHelper.Chains;
using OccultCrescentHelper.Data;
using OccultCrescentHelper.Enums;
using OccultCrescentHelper.Modules.CriticalEncounters;
using OccultCrescentHelper.Modules.Fates;
using OccultCrescentHelper.Modules.StateManager;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;

namespace OccultCrescentHelper.Modules.Automator;


public class Activity
{
    public readonly EventData data;

    private Lifestream lifestream;

    private VNavmesh vnav;

    private AutomatorModule module;

    public Func<bool> isValid = () => true;

    public Func<Vector3> getPosition = () => Vector3.Zero;

    public ActivityState state = ActivityState.Idle;

    private Dictionary<ActivityState, Func<StateManagerModule, Func<Chain>?>> handlers;

    private Activity(EventData data, Lifestream lifestream, VNavmesh vnav, AutomatorModule module)
    {
        this.data = data;
        this.lifestream = lifestream;
        this.vnav = vnav;
        this.module = module;

        handlers = new() {
            { ActivityState.Idle, GetIdleChain },
            { ActivityState.Pathfinding, GetPathfindingChain },
            { ActivityState.WaitingToStartCriticalEncounter, GetWaitingToStartCriticalEncounterChain },
            { ActivityState.Participating, GetParticipatingChain },
            { ActivityState.Done, GetDoneChain },
        };

        var states = module.GetModule<StateManagerModule>();
        if (states.GetState() == State.InFate || states.GetState() == State.InCriticalEncounter)
        {
            this.state = ActivityState.Participating;
        }
    }

    public static Activity ForCriticalEncounter(DynamicEvent encounter, EventData data, Lifestream lifestream, VNavmesh vnav, AutomatorModule module, CriticalEncountersModule critical)
    {
        return new(data, lifestream, vnav, module) {
            isValid = () => critical.criticalEncounters[data.id].State != DynamicEventState.Inactive,
            getPosition = () => encounter.MapMarker.Position,
        };
    }

    public static Activity ForFate(IFate fate, EventData data, Lifestream lifestream, VNavmesh vnav, AutomatorModule module)
    {
        return new(data, lifestream, vnav, module) {
            isValid = () => Svc.Fates.Contains(fate),
            getPosition = () => data.start ?? fate.Position,
        };
    }

    public unsafe Func<Chain>? GetChain(StateManagerModule states)
    {
        if (!isValid())
        {
            return null;
        }

        return handlers[state](states);
    }

    public Func<Chain>? GetIdleChain(StateManagerModule states)
    {
        return () => {
            return Chain.Create("Illegal:Idle")
                .ConditionalThen(_ => module.config.ShouldToggleAiProvider && !Svc.Condition[ConditionFlag.InCombat], _ => module.config.AiProvider.Off())
                .Then(_ => vnav.Stop())
                .Then(_ => state = ActivityState.Pathfinding);
        };
    }

    public Func<Chain>? GetPathfindingChain(StateManagerModule states)
    {
        return () => {
            var yes = module.GetIPCProvider<YesAlready>();

            var playerShard = AethernetData.All().OrderBy((data) => Vector3.Distance(Player.Position, data.position)).First();
            var activityShard = GetAethernetData();

            bool isFate = data.type == EventType.Fate;
            var navType = SmartNavigation.Decide(Player.Position, getPosition(), activityShard);

            var chain = Chain.Create("Illegal:Pathfinding")
                .ConditionalWait(_ => !isFate && module.config.ShouldDelayCriticalEncounters, Random.Shared.Next(10000, 15001));

            switch (navType)
            {
                case NavigationType.WalkToEvent:
                    chain
                        .ConditionalThen(_ => module._config.TeleporterConfig.ShouldMount, ChainHelper.MountChain())
                        .Then(new PathfindingChain(vnav, getPosition(), data, false));
                    break;

                case NavigationType.ReturnThenWalkToEvent:
                    chain
                        .Then(ChainHelper.ReturnChain())
                        .ConditionalThen(_ => module._config.TeleporterConfig.ShouldMount, ChainHelper.MountChain())
                        .Then(new PathfindingChain(vnav, getPosition(), data, false));
                    break;

                case NavigationType.ReturnThenTeleportToEventshard:
                    chain
                        .Then(ChainHelper.ReturnChain())
                        .Then(ChainHelper.TeleportChain(activityShard.aethernet))
                        .Debug("Waiting for lifestream to not be 'busy'")
                        .Then(new TaskManagerTask(() => !lifestream.IsBusy(), new() { TimeLimitMS = 30000 }))
                        .Then(new PathfindingChain(vnav, getPosition(), data, false));
                    break;

                case NavigationType.WalkToClosestShardAndTeleportToEventShardThenWalkToEvent:
                    chain
                        .ConditionalThen(_ => module._config.TeleporterConfig.ShouldMount, ChainHelper.MountChain())
                        .Then(new PathfindingChain(vnav, playerShard.position, data, false))
                        .WaitUntilNear(vnav, playerShard.position, 4f)
                        .Then(ChainHelper.TeleportChain(activityShard.aethernet))
                        .Debug("Waiting for lifestream to not be 'busy'")
                        .Then(new TaskManagerTask(() => !lifestream.IsBusy(), new() { TimeLimitMS = 30000 }))
                        .Then(new PathfindingChain(vnav, getPosition(), data, false));
                    break;
            }

            chain
                // Fate
                .ConditionalThen(_ => isFate, GetFatePathfindingWatcher(states, vnav))
                .ConditionalThen(_ => isFate, _ => state = ActivityState.Participating)
                // Critical Encounter
                .ConditionalThen(_ => !isFate, GetCriticalEncounterPathfindingWatcher(states, vnav))
                .ConditionalThen(_ => !isFate, _ => state = ActivityState.WaitingToStartCriticalEncounter);

            return chain;
        };
    }

    public unsafe Func<Chain>? GetWaitingToStartCriticalEncounterChain(StateManagerModule states)
    {
        return () => {
            return Chain.Create("Illegal:WaitingToStartCriticalEncounter")
                .Then(new TaskManagerTask(() => {
                    if (!isValid())
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
                        if (Svc.Condition[ConditionFlag.Mounted])
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

    public Func<Chain>? GetParticipatingChain(StateManagerModule states)
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

    public Func<Chain>? GetDoneChain(StateManagerModule states)
    {
        return null;
    }

    private unsafe TaskManagerTask GetFatePathfindingWatcher(StateManagerModule states, VNavmesh vnav)
    {
        Vector3 lastTargetPos = Vector3.Zero;

        return new(() => {
            if (EzThrottler.Throttle("FatePathfindingWatcher.EnemyScan", 100))
            {
                Svc.Targets.Target ??= GetClosestEnemy();
            }

            // module.GetModule<MountModule>().MaintainMount();

            var target = Svc.Targets.Target;
            if (target != null)
            {
                if (Vector3.Distance(target.Position, lastTargetPos) > 5f)
                {
                    vnav.PathfindAndMoveTo(target.Position, false);
                    lastTargetPos = target.Position;
                }

                if (states.GetState() == State.InFate)
                {
                    if (Vector3.Distance(Player.Position, target.Position) <= module.config.EngagementRange)
                    {
                        // Dismount
                        if (Svc.Condition[ConditionFlag.Mounted])
                        {
                            ActionManager.Instance()->UseAction(ActionType.Mount, module.plugin.config.MountConfig.Mount);
                        }

                        vnav.Stop();

                        return true;
                    }
                }
            }

            if (!vnav.IsRunning())
            {
                throw new VnavmeshStoppedException();
            }

            return false;
        }, new() { TimeLimitMS = 180000, ShowError = false });
    }

    private TaskManagerTask GetCriticalEncounterPathfindingWatcher(StateManagerModule states, VNavmesh vnav)
    {
        return new(() => {
            if (!isValid())
            {
                throw new Exception("Activity is no longer valid.");
            }

            // module.GetModule<MountModule>().MaintainMount();

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

    private int GetEnemyCount()
    {
        return GetEnemies().Count();
    }

    private IGameObject? GetClosestEnemy()
    {
        return GetEnemies().FirstOrDefault();
    }



    private IGameObject? GetCentralMostEnemy()
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
        => data.aethernet?.GetData() ?? AethernetData.All().OrderBy((data) => Vector3.Distance(getPosition(), data.position)).First();

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

        return Vector3.Distance(Player.Position, getPosition()) <= radius;
    }
}
