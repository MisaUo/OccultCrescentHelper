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
    public Dictionary<uint, IFate> fates = [];

    public Dictionary<uint, EventProgress> progress { get; } = [];

    public event Action<IFate>? OnFateSpawned;

    public event Action<IFate>? OnFateDespawned;


    public void Tick(IFramework _)
    {
        var currentFates = Svc.Fates.ToDictionary(f => (uint)f.FateId, f => f);

        foreach (var (id, fate) in currentFates)
        {
            if (!fates.ContainsKey(id))
            {
                OnFateSpawned?.Invoke(fate);
            }

            fates[id] = fate;
        }

        var despawned = fates.Keys.Except(currentFates.Keys).ToList();
        foreach (var id in despawned)
        {
            OnFateDespawned?.Invoke(fates[id]);
            fates.Remove(id);
            progress.Remove(id);
        }

        foreach (var (id, fate) in fates)
        {
            if (fate.Progress == 0)
            {
                continue;
            }

            if (!progress.TryGetValue(id, out var prog))
            {
                prog = new EventProgress();
                progress[id] = prog;
            }

            if (prog.samples.Count == 0 || prog.samples[^1].Progress != fate.Progress)
            {
                prog.AddProgress(fate.Progress);
            }

            if (fate.Progress == 100)
            {
                progress.Remove(id);
            }
        }
    }
}
