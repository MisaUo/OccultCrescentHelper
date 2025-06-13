using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using OccultCrescentHelper.Data;

namespace OccultCrescentHelper.Modules.Carrots;

public class Radar
{
    public void Draw(CarrotsModule module)
    {
        if (!ZoneData.IsInOccultCrescent())
        {
            return;
        }

        if (!module.config.ShouldDrawLineToCarrots)
        {
            return;
        }

        if (Svc.ClientState.LocalPlayer == null || Svc.Condition[ConditionFlag.InCombat])
        {
            return;
        }

        var pos = Svc.ClientState.LocalPlayer!.Position;

        foreach (var carrot in module.carrots)
        {
            if (!carrot.IsValid())
            {
                continue;
            }

            Helpers.DrawLine(pos, carrot.GetPosition(), 3f, Carrot.color);
        }
    }
}
