using System;
using System.Collections.Generic;
using System.Linq;
using BOCCHI.Data;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;

namespace BOCCHI.Modules.Fates;

public class FateTracker
{
    public readonly Dictionary<uint, IFate> Fates = [];

    public Dictionary<uint, EventProgress> Progress { get; } = [];

    public event Action<IFate>? OnFateSpawned;

    public event Action<IFate>? OnFateDespawned;


    public void Tick(IFramework _)
    {
        var currentFates = Svc.Fates.ToDictionary(f => (uint)f.FateId, f => f);

        foreach (var (id, fate) in currentFates)
        {
            if (!Fates.ContainsKey(id))
            {
                OnFateSpawned?.Invoke(fate);
            }

            Fates[id] = fate;
        }

        var despawned = Fates.Keys.Except(currentFates.Keys).ToList();
        foreach (var id in despawned)
        {
            OnFateDespawned?.Invoke(Fates[id]);
            Fates.Remove(id);
            Progress.Remove(id);
        }

        foreach (var (id, fate) in Fates)
        {
            if (fate.Progress == 0)
            {
                continue;
            }

            if (!Progress.TryGetValue(id, out var current))
            {
                current = new EventProgress();
                Progress[id] = current;
            }

            if (current.samples.Count == 0 || current.samples[^1].Progress != fate.Progress)
            {
                current.AddProgress(fate.Progress);
            }

            if (fate.Progress == 100)
            {
                Progress.Remove(id);
            }
        }
    }
}
