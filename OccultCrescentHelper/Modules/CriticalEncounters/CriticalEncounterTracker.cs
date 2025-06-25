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
    public Dictionary<uint, DynamicEvent> criticalEncounters = [];

    public Dictionary<uint, EventProgress> progress { get; } = [];

    public unsafe void Tick(IFramework _)
    {
        var pos = Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero;

        criticalEncounters = PublicContentOccultCrescent.GetInstance()->DynamicEventContainer.Events
            .ToArray()
            .ToDictionary(ev => (uint)ev.DynamicEventId, ev => ev);

        foreach (var ev in criticalEncounters.Values)
        {
            if (ev.State == DynamicEventState.Battle)
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
        }
    }
}
