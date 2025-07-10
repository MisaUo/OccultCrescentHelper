using System.Collections.Generic;
using System.Linq;
using BOCCHI.Data;
using BOCCHI.Modules.Buff.Chains;
using Dalamud.Plugin.Services;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using Ocelot.Chain;

namespace BOCCHI.Modules.Buff;

public class BuffManager
{
    private bool applyBuffsOnNextTick = false;

    public void QueueBuffs()
    {
        applyBuffsOnNextTick = true;
    }

    public bool IsQueued()
    {
        return applyBuffsOnNextTick;
    }

    private int lowestTimer = int.MaxValue;

    public void Tick(IFramework _, BuffModule module)
    {
        if (applyBuffsOnNextTick)
        {
            applyBuffsOnNextTick = false;
            ApplyBuffs(module);
        }

        if (EzThrottler.Throttle("BuffManager.Tick.GetLowestBuffTimer", 1000))
        {
            lowestTimer = GetLowestBuffTimer(module);
        }
    }

    public void ApplyBuffs(BuffModule module)
    {
        var manager = ChainManager.Get("OCH##BuffManager");
        if (manager.IsRunning)
        {
            return;
        }

        manager.Submit(new AllBuffsChain(module));
    }

    private int GetLowestBuffTimer(BuffModule module)
    {
        List<uint> buffs = [];

        if (module.config.ApplyEnduringFortitude)
        {
            buffs.Add((uint)PlayerStatus.EnduringFortitude);
        }

        if (module.config.ApplyFleetfooted)
        {
            buffs.Add((uint)PlayerStatus.Fleetfooted);
        }

        if (module.config.ApplyRomeosBallad)
        {
            buffs.Add((uint)PlayerStatus.RomeosBallad);
        }

        var statuses = Player.Status.Where(s => buffs.Contains(s.StatusId)).ToList();
        return statuses.Count == 0 ? 0 : statuses.Select(status => (int)status.RemainingTime).Min();
    }

    public bool ShouldRefresh(BuffModule module)
    {
        if (module.enabled == false)
        {
            return false;
        }

        return lowestTimer <= module.config.ReapplyThreshold * 60;
    }
}
