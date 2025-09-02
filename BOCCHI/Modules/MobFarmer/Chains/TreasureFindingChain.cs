using BOCCHI.ActionHelpers;
using BOCCHI.Data;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.MobFarmer.Chains;

public class TreasureFindingChain(MobFarmerModule module) : ChainFactory
{
    protected override Chain Create(Chain chain)
    {
        chain.BreakIf(() => Actions.Freelancer.Treasuresight.GetRecastTime() >= module.Config.MaximumBattleBellWaitTime);
        chain.Then(_ => Actions.TryUnmount());
        chain.Then(Job.Freelancer.ChangeToChain).Wait(500);
        chain.Then(Actions.Freelancer.Treasuresight.GetCastChain()).Wait(1500);
        chain.Then(Job.Cannoneer.ChangeToChain);
        return chain;
    }
}
