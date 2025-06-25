using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using OccultCrescentHelper.Enums;

namespace OccultCrescentHelper.Modules.Carrots;

public class CarrotsTracker
{
    public List<Carrot> carrots = [];

    public void Tick(IFramework _, Plugin plugin)
    {
        var pos = Svc.ClientState.LocalPlayer!.Position;

        carrots = Svc
            .Objects.Where(o => o != null)
            .Where(o => o.ObjectKind == ObjectKind.EventObj)
            .Where(o => o.DataId == (uint)OccultObjectType.Carrot)
            .OrderBy(o => Vector3.Distance(o.Position, pos))
            .Select(o => new Carrot(o))
            .Where(c => c.IsValid())
            .ToList();
    }
}
