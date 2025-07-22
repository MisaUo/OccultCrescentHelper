using System.Linq;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using Ocelot.IPC;
using Ocelot.States;

namespace BOCCHI.Modules.MobFarmer.States;

[State<FarmerPhase>(FarmerPhase.Stacking)]
public class StackingHandler : FarmerPhaseHandler
{
    private bool HasRunStack = false;

    public override void OnEnter(MobFarmerModule module)
    {
        HasRunStack = false;
    }

    public override FarmerPhase? Handle(MobFarmerModule module)
    {
        var vnav = module.GetIPCProvider<VNavmesh>();

        if (HasRunStack && !vnav.IsRunning())
        {
            HasRunStack = false;
            module.Farmer.RotationPlugin.PhantomJobOn();
            return FarmerPhase.Fighting;
        }

        var furthest = module.Scanner.InCombat.Where(o => o.Address != Svc.Targets.Target?.Address).OrderBy(Player.DistanceTo).LastOrDefault();
        if (furthest == null)
        {
            return FarmerPhase.Fighting;
        }

        vnav.PathfindAndMoveTo(furthest.Position, false);
        HasRunStack = true;

        return null;
    }
}
