using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using ECommons.DalamudServices;
using OccultCrescentHelper.Data;
using Ocelot.IPC;

namespace OccultCrescentHelper.Modules.Automator;

public class Fate : Activity
{
    private IFate fate;

    public Fate(EventData data, Lifestream lifestream, VNavmesh vnav, AutomatorModule module, IFate fate)
        : base(data, lifestream, vnav, module)
    {
        this.fate = fate;
    }

    public override bool IsValid() => Svc.Fates.Contains(fate);

    public override Vector3 GetPosition() => data.start ?? fate.Position;
}
