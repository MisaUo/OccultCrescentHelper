using BOCCHI.ActionHelpers;
using BOCCHI.Data;
using BOCCHI.Modules.CriticalEncounters;
using BOCCHI.Modules.StateManager;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Chain;
using Ocelot.IPC;
using System;
using System.Linq;
using System.Numerics;

namespace BOCCHI.Modules.Automator;

public class CriticalEncounter : Activity
{
    private readonly CriticalEncountersModule source;

    private DynamicEvent Encounter
    {
        get => source.CriticalEncounters[data.Id];
    }

    private bool finalDestination = false;

    public CriticalEncounter(EventData data, Lifestream lifestream, VNavmesh vnav, AutomatorModule module, CriticalEncountersModule source)
        : base(data, lifestream, vnav, module)
    {
        this.source = source;

        handlers.Add(ActivityState.WaitingToStartCriticalEncounter, GetWaitingToStartCriticalEncounterChain);
    }

    protected override TaskManagerTask GetPathfindingWatcher(StateManagerModule states)
    {
        return new TaskManagerTask(() =>
        {
            if (!IsValid())
            {
                throw new Exception("Activity is no longer valid.");
            }

            if (!finalDestination && IsCloseToZone())
            {
                // Get all players in the zone
                var playersInZone = Svc.Objects
                    .Where(o => o.ObjectKind == ObjectKind.Player)
                    .Where(o => Vector3.Distance(o.Position, GetPosition()) <= GetRadius())
                    .ToList();

                if (playersInZone.Count >= 1)
                {
                    var random = new Random();
                    var center = GetPosition();
                    
                    var minR = MathF.Max(0f, module.Config.MinDistance);
                    var maxR = MathF.Max(minR, module.Config.MaxDistance);
                    
                    var theta = random.NextDouble() * 2.0 * Math.PI;
                    
                    var u = random.NextDouble();
                    var r = MathF.Sqrt((float)(u * (maxR * maxR - minR * minR) + minR * minR));
                    
                    var valueX = r * (float)Math.Cos(theta);
                    var valueZ = r * (float)Math.Sin(theta);
                    
                    var destination = center + new Vector3(valueX, 0f, valueZ);
                    Svc.Log.Debug($"获取场地中心为{center}, 本次偏移为{new Vector3(valueX, 0f, valueZ)}, 最终目的地为{destination}");
                    module.Debug($"Pathfinding to random point: {destination}");
                    vnav.PathfindAndMoveTo(destination, false);

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
            var encounter = critical.CriticalEncounters[data.Id];

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
        }, new TaskManagerConfiguration { TimeLimitMS = 180000, ShowError = false });
    }


    private Func<Chain> GetWaitingToStartCriticalEncounterChain(StateManagerModule states)
    {
        return () =>
        {
            return Chain.Create("Illegal:WaitingToStartCriticalEncounter")
                .Then(new TaskManagerTask(() =>
                    {
                        if (!IsValid())
                        {
                            throw new Exception("The critical encounter appears to have started without you.");
                        }

                        var critical = module.GetModule<CriticalEncountersModule>();
                        var encounter = critical.CriticalEncounters[data.Id];

                        if (encounter.State == DynamicEventState.Battle &&
                            states.GetState() != State.InCriticalEncounter)
                        {
                            throw new Exception("The critical encounter appears to have started without you.");
                        }

                        if (!vnav.IsRunning() && states.GetState() == State.InCombat)
                        {
                            Actions.TryUnmount();

                            if (module.Config.ShouldToggleAiProvider)
                            {
                                module.Config.AiProvider.On();
                            }

                            Chat.ExecuteCommand("/aeTargetSelector off");
                        }

                        return states.GetState() == State.InCriticalEncounter;
                    },
                    new TaskManagerConfiguration
                    {
                        TimeLimitMS = 180000,
                    }))
                .Then(_ => state = ActivityState.Participating);
        };
    }

    public override unsafe bool IsValid()
    {
        if (Encounter.State == DynamicEventState.Register)
        {
            return true;
        }

        var dec = DynamicEventContainer.GetInstance();
        return dec != null && Encounter.DynamicEventId == dec->CurrentEventId;
    }

    protected override float GetRadius()
    {
        // This is kind of an assumption, but it seems accurate enough for most encounters.
        return Encounter.Unknown4;
    }

    protected override Vector3 GetPosition()
    {
        return Encounter.MapMarker.Position;
    }

    public override string GetName()
    {
        return Encounter.Name.ToString();
    }

    private bool IsCloseToZone(float radius = 50f)
    {
        return Player.DistanceTo(GetPosition()) <= radius;
    }


    protected override unsafe bool IsActivityTarget(IBattleNpc obj)
    {
        try
        {
            var battleChara = (BattleChara*)obj.Address;

            var isRelatedToCurrentEvent = battleChara->EventId.EntryId == Player.BattleChara->EventId.EntryId;

            return obj.SubKind == (byte)BattleNpcSubKind.Enemy && isRelatedToCurrentEvent;
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex.Message);
            return false;
        }
    }

    protected override ActivityState GetPostPathfindingState()
    {
        return ActivityState.WaitingToStartCriticalEncounter;
    }
}
