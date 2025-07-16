﻿using System;
using System.Collections.Generic;
using BOCCHI.Data;
using BOCCHI.Enums;
using ECommons.DalamudServices;
using Ocelot.IPC;
using Ocelot.Modules;

namespace BOCCHI.Modules.ForkedTower;

public class Wrath : IRotationPlugin
{
    private WrathCombo wrath;

    private Guid lease;

    private Dictionary<JobId, string> WrathOptions = new()
    {
        { JobId.Cannoneer, "Phantom_Cannoneer" },
    };

    public Wrath(IModule module)
    {
        wrath = module.GetIPCProvider<WrathCombo>();
        var lease = wrath.RegisterForLease(Svc.PluginInterface.InternalName, module.GetType().FullName!);
        if (lease == null)
        {
            throw new Exception("Unable to create Wrath Combo");
        }

        this.lease = (Guid)lease;
    }

    public void PhantomJobOn(Job? job = null)
    {
        job ??= Job.Current;

        if (!WrathOptions.TryGetValue(job.id, out var option))
        {
            return;
        }

        wrath.SetComboOptionState(lease, option.ToString(), true);
    }

    public void PhantomJobOff(Job? job = null)
    {
        job ??= Job.Current;

        if (!WrathOptions.TryGetValue(job.id, out var option))
        {
            return;
        }

        wrath.SetComboOptionState(lease, option, false);
    }

    void IDisposable.Dispose()
    {
        wrath.ReleaseControl(lease);
    }
}
