using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using Ocelot.IPC;
using Ocelot.States;

namespace BOCCHI.Modules.MobFarmer.States;

[State<FarmerPhase>(FarmerPhase.Fighting)]
public class FightingHandler : FarmerPhaseHandler
{
    public override FarmerPhase? Handle(MobFarmerModule module)
    {
        var anyInCombat = module.Scanner.InCombat.Any();
        if (anyInCombat && EzThrottler.Throttle("Targetter"))
        {
            Svc.Targets.Target = module.Scanner.InCombat.Centroid();
        }


        var startingPoint = module.Farmer.StartingPoint;
        var shouldReturnHome = module.Config.ReturnToStartInWaitingPhase && Player.DistanceTo(startingPoint) >= module.Config.MinEuclideanDistanceToReturnHome;
        if (shouldReturnHome && !anyInCombat)
        {
            var vnav = module.GetIPCProvider<VNavmesh>();
            if (!vnav.IsRunning())
            {
                vnav.PathfindAndMoveTo(startingPoint, false);
            }

            return Player.DistanceTo(startingPoint) <= 2f ? FarmerPhase.Waiting : null;
        }

        if (!anyInCombat && !Svc.Condition[ConditionFlag.InCombat])
        {
            module.Farmer.RotationPlugin.PhantomJobOff();
            return FarmerPhase.Waiting;
        }

        return null;
    }
}
