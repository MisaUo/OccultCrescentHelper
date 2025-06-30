using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BOCCHI.Data;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace BOCCHI.Modules.CriticalEncounters;

public class CriticalEncounterTracker
{
    public Dictionary<uint, DynamicEvent> criticalEncounters = new();

    public Dictionary<uint, EventProgress> progress { get; } = new();

    public DateTime LastForkedTower { get; private set; } = DateTime.Now;

    // Store last known states of each event by ID
    private readonly Dictionary<uint, DynamicEventState> lastStates = new();

    public unsafe CriticalEncounterTracker()
    {
        criticalEncounters = PublicContentOccultCrescent.GetInstance()->DynamicEventContainer.Events
            .ToArray()
            .ToDictionary(ev => (uint)ev.DynamicEventId, ev => ev);

        OnInactiveState += ev =>
        {
            if (ev.EventType < 4)
            {
                return;
            }

            LastForkedTower = DateTime.Now;
        };
    }

    public event Action<DynamicEvent>? OnInactiveState;

    public event Action<DynamicEvent>? OnRegisterState;

    public event Action<DynamicEvent>? OnWarmupState;

    public event Action<DynamicEvent>? OnBattleState;


    public unsafe void Tick(IFramework _)
    {
        var pos = Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero;

        criticalEncounters = PublicContentOccultCrescent.GetInstance()->DynamicEventContainer.Events
            .ToArray()
            .ToDictionary(ev => (uint)ev.DynamicEventId, ev => ev);

        foreach (var ev in criticalEncounters.Values)
        {
            // Get previous state, default to Inactive if unknown
            lastStates.TryGetValue(ev.DynamicEventId, out var previousState);

            var currentState = ev.State;

            if (currentState == DynamicEventState.Battle)
            {
                if (ev.Progress == 0)
                {
                    continue;
                }

                if (!this.progress.TryGetValue(ev.DynamicEventId, out var progress))
                {
                    progress = new EventProgress();
                    this.progress[ev.DynamicEventId] = progress;
                }

                if (progress.samples.Count == 0 || progress.samples[^1].Progress != ev.Progress)
                {
                    progress.AddProgress(ev.Progress);
                }

                if (ev.Progress == 100)
                {
                    this.progress.Remove(ev.DynamicEventId);
                }
            }
            else
            {
                progress.Remove(ev.DynamicEventId);
            }

            if (previousState == currentState)
            {
                continue;
            }

            lastStates[ev.DynamicEventId] = currentState;

            switch (currentState)
            {
                case DynamicEventState.Inactive:
                    OnInactiveState?.Invoke(ev);
                    break;

                case DynamicEventState.Register:
                    OnRegisterState?.Invoke(ev);
                    break;

                case DynamicEventState.Warmup:
                    OnWarmupState?.Invoke(ev);
                    break;

                case DynamicEventState.Battle:
                    OnBattleState?.Invoke(ev);
                    break;
            }
        }
    }
}
