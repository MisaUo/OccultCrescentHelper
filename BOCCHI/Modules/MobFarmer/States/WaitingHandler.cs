using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using Ocelot.States;

namespace BOCCHI.Modules.MobFarmer.States;

[State<FarmerPhase>(FarmerPhase.Waiting)]
public class WaitingHandler : FarmerPhaseHandler
{
    public override FarmerPhase? Handle(MobFarmerModule module)
    {
        if (Svc.Condition[ConditionFlag.InCombat])
        {
            return FarmerPhase.Fighting;
        }

        var mobs = module.Scanner.Mobs;

        return mobs.Count() >= module.Config.MinimumMobsToStartLoop ? FarmerPhase.Buffing : null;
    }
}
