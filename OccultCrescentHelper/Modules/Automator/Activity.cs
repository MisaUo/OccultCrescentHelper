using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation;
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
            { ActivityState.WaitingToStartCriticalEncoutner, GetWaitingToStartCriticalEncoutnerChain },
            { ActivityState.Participating, GetParticipatingChain },
            { ActivityState.Done, GetDoneChain },
        };
    }

    public static Activity ForCriticalEncounter(DynamicEvent encounter, EventData data, Lifestream lifestream, VNavmesh vnav, AutomatorModule module, CriticalEncountersModule critical)
    {
        return new(data, lifestream, vnav, module) {
            isValid = () => critical.criticalEncounters[(int)data.id].State != DynamicEventState.Inactive,
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
            return () => Chain.Create("Dummy Chain");
        }

        return handlers[state](states);
    }

    public Func<Chain>? GetIdleChain(StateManagerModule states)
    {
        return () => {
            return Chain.Create("Illegal:Idle")
                .ConditionalWait(_ => states.GetState() != State.Idle, 1000)
                .RunIf(() => states.GetState() == State.Idle)
                .ConditionalThen(_ => module.config.ShouldToggleAiProvider, _ => module.config.AiProvider.Off())
                .Then(_ => vnav.Stop())
                .ConditionalThen(_ => ShouldReturn(), _ => new ReturnChain(new Vector3(830.75f, 72.98f, -695.98f), module.GetIPCProvider<YesAlready>(), vnav, approachAetherye: true))
                .Then(_ => state = ActivityState.Pathfinding);
        };
    }

    public Func<Chain>? GetPathfindingChain(StateManagerModule states)
    {
        return () => {
            var playerShard = AethernetData.All().OrderBy((data) => Vector3.Distance(Player.Position, data.position)).First();
            var activityShard = GetAethernetData();

            bool isFate = data.type == EventType.Fate;

            var chain = Chain.Create("Illegal:Pathfinding")
                .ConditionalWait(_ => !isFate && module.config.ShouldDelayCriticalEncounters, Random.Shared.Next(10000, 15001))
                .Then(_ => vnav.Stop())
                .ConditionalThen(
                    _ => playerShard.dataId != activityShard.dataId && playerShard.Distance() > 4f,
                    _ => new TaskManagerTask(() => {
                        if (!vnav.IsRunning())
                        {
                            vnav.PathfindAndMoveTo(playerShard.position, false);
                        }

                        return playerShard.Distance() < 4f;
                    })
                )
                .ConditionalThen(_ => playerShard.dataId != activityShard.dataId, new TeleportChain(lifestream, activityShard.aethernet))
                .Then(_ => ChainManager.Get("Mounter").Submit(() => Chain.Create().Then(
                    new MountChain(module.plugin.config.TeleporterConfig.Mount)
                )))
                .Then(new PathfindingChain(vnav, getPosition(), data, false, 20f, 10f))
                .WaitToStartPathfinding(vnav);

            if (isFate)
            {
                chain
                    .Then(GetFatePathfindingWatcher(states, vnav))
                    .Then(_ => state = ActivityState.Participating);
            }
            else
            {
                chain
                    .Then(GetCriticalEncounterPathfindingWatcher(states, vnav))
                    .Then(_ => state = ActivityState.WaitingToStartCriticalEncoutner);
            }


            return chain;
        };
    }

    public Func<Chain>? GetWaitingToStartCriticalEncoutnerChain(StateManagerModule states)
    {
        return () => {
            return Chain.Create("Illegal:WaitingToStartCriticalEncoutner")
                .Then(new TaskManagerTask(() => {
                    if (!isValid())
                    {
                        throw new Exception("The critical encoutner appeared to start without you");
                    }

                    return states.GetState() == State.InCriticalEncounter;
                }, new() {
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
                        Svc.Targets.Target ??= GetClosestEnemy();
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
        bool runningToTarget = false;
        return new(() => {
            if (EzThrottler.Throttle("FatePathfindingWatcher.EnemyScan", 100))
            {
                Svc.Targets.Target ??= GetClosestEnemy();
            }

            if (Svc.Targets.Target != null)
            {
                if (!runningToTarget)
                {
                    vnav.PathfindAndMoveTo(Svc.Targets.Target.Position, false);
                    runningToTarget = true;
                }

                if (states.GetState() == State.InFate)
                {
                    if (Vector3.Distance(Player.Position, Svc.Targets.Target.Position) <= module.config.EngagementRange)
                    {
                        // Dismount
                        if (Svc.Condition[ConditionFlag.Mounted])
                        {
                            ActionManager.Instance()->UseAction(ActionType.Mount, module.plugin.config.TeleporterConfig.Mount);
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
            if (!vnav.IsRunning() && IsInZone())
            {
                return true;
            }

            if (!vnav.IsRunning())
            {
                throw new VnavmeshStoppedException();
            }

            if (!isValid())
            {
                throw new Exception("Activity is no longer valid.");
            }

            return false;
        }, new() { TimeLimitMS = 180000, ShowError = false });
    }

    private IGameObject? GetClosestEnemy()
    {
        return Svc.Objects
            .Where(o =>
                o != null &&
                o.ObjectKind == ObjectKind.BattleNpc &&
                IsActivityTarget(o) &&
                o.IsTargetable
            )
            .OrderBy(o => Vector3.Distance(o.Position, Player.Position))
            .ToList()
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
        return Vector3.Distance(Player.Position, getPosition()) <= 20f;
    }

    private bool ShouldReturn()
    {
        var goal = getPosition();

        AethernetData closest = AethernetData.GetClosestTo(goal);

        var aethernetDistance = Vector3.Distance(goal, closest.position);
        var playerDistance = Vector3.Distance(goal, Player.Position);

        return aethernetDistance < playerDistance;
    }
}
