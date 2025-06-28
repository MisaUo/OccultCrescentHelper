using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BOCCHI.Data;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;

namespace BOCCHI.Modules.Fates;

public class FateTracker
{
    public Dictionary<uint, IFate> fates = [];

    public Dictionary<uint, EventProgress> progress { get; } = [];

    public void Tick(IFramework _)
    {
        var pos = Svc.ClientState.LocalPlayer!.Position;

        fates = Svc.Fates.OrderBy(f => Vector3.Distance(f.Position, pos)).ToDictionary(f => (uint)f.FateId, f => f);

        foreach (var fate in fates.Values)
        {
            if (fate.Progress == 0)
            {
                continue;
            }

            if (!this.progress.TryGetValue(fate.FateId, out var progress))
            {
                progress = new EventProgress();
                this.progress[fate.FateId] = progress;
            }

            if (progress.samples.Count == 0 || progress.samples[^1].Progress != fate.Progress)
            {
                progress.AddProgress(fate.Progress);
            }

            if (fate.Progress == 100)
            {
                this.progress.Remove(fate.FateId);
            }
        }


        // Remove non-active fates
        var active = fates.Select(f => f.Key).ToHashSet();
        var obsolete = progress.Keys.Where(id => !active.Contains(id)).ToList();
        foreach (var id in obsolete)
        {
            progress.Remove(id);
        }
    }
}
