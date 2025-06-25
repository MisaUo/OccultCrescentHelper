using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.Throttlers;
using OccultCrescentHelper.Data;
using OccultCrescentHelper.Modules.Buff.Chains;
using Ocelot.Chain;

namespace OccultCrescentHelper.Modules.Buff;

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

    public int lowestTimer = int.MaxValue;

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

    public unsafe void ApplyBuffs(BuffModule module)
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
        var player = Svc.ClientState.LocalPlayer;
        if (player == null)
        {
            // Prevent rebuffing if we can't access our player
            return int.MaxValue;
        }

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

        var statuses = player.StatusList.Where(s => buffs.Contains(s.StatusId)).ToList();
        if (statuses.Count == 0)
        {
            return 0;
        }

        var min = int.MaxValue;
        foreach (var status in statuses)
        {
            min = Math.Min((int)status.RemainingTime, min);
        }

        return min;
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
