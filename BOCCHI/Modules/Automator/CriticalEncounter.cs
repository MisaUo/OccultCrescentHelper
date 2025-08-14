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

                if (playersInZone.Count > 2)
                {
                    var minX = playersInZone.Min(p => p.Position.X);
                    var maxX = playersInZone.Max(p => p.Position.X);
                    var minY = playersInZone.Min(p => p.Position.Z);
                    var maxY = playersInZone.Max(p => p.Position.Z);


                    // Choose a random point within the bounding box of players
                    var random = new Random();
                    const float minAbs = 1f;
                    const float maxAbs = 4f;

                    var positiveX = random.Next(0, 2) == 1;
                    var magnitudeX = (float)(minAbs + random.NextDouble() * (maxAbs - minAbs));
                    var valueX = positiveX ? magnitudeX : -magnitudeX;

                    var positiveZ = random.Next(0, 2) == 1;
                    var magnitudeZ = (float)(minAbs + random.NextDouble() * (maxAbs - minAbs));
                    var valueZ = positiveZ ? magnitudeZ : -magnitudeZ;

                    var destination = GetPosition() + new Vector3(valueX, 0, valueZ);
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

        if (DynamicEventContainer.GetInstance()->CurrentEventId == Encounter.DynamicEventId)
        {
            return true;
        }

        if (Encounter.State is DynamicEventState.Warmup or DynamicEventState.Battle)
        {
            return Player.Status.Has(PlayerStatus.HoofingIt);
        }

        return false;
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
