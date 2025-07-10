using System;
using System.Collections.Generic;
using System.Linq;
using BOCCHI.Data;
using BOCCHI.Modules.Fates;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace BOCCHI.Modules.CriticalEncounters;

public class CriticalEncounterTracker
{
    public Dictionary<uint, DynamicEvent> CriticalEncounters = new();

    public Dictionary<uint, EventProgress> Progress { get; } = new();

    public TowerTimer TowerTimer { get; private set; }

    // Store last known states of each event by ID
    private readonly Dictionary<uint, DynamicEventState> lastStates = new();

    public CriticalEncounterTracker(CriticalEncountersModule module)
    {
        TowerTimer = new TowerTimer(this, module.GetModule<FatesModule>());
    }

    public event Action<DynamicEvent>? OnInactiveState;

    public event Action<DynamicEvent>? OnRegisterState;

    public event Action<DynamicEvent>? OnWarmupState;

    public event Action<DynamicEvent>? OnBattleState;


    public unsafe void Tick(IFramework _)
    {
        CriticalEncounters = PublicContentOccultCrescent.GetInstance()->DynamicEventContainer.Events
            .ToArray()
            .ToDictionary(ev => (uint)ev.DynamicEventId, ev => ev);

        foreach (var ev in CriticalEncounters.Values)
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

                if (!Progress.TryGetValue(ev.DynamicEventId, out var current))
                {
                    current = new EventProgress();
                    Progress[ev.DynamicEventId] = current;
                }

                if (current.samples.Count == 0 || current.samples[^1].Progress != ev.Progress)
                {
                    current.AddProgress(ev.Progress);
                }

                if (ev.Progress == 100)
                {
                    Progress.Remove(ev.DynamicEventId);
                }
            }
            else
            {
                Progress.Remove(ev.DynamicEventId);
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
