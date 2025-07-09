using BOCCHI.ActionHelpers;
using BOCCHI.Data;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Buff.Chains;

public class MonkBuffChain(BuffModule module) : BuffChain(Job.Monk, PlayerStatus.Fleetfooted, Actions.Monk.Counterstance)
{
    protected override Chain Create(Chain chain)
    {
        chain.RunIf(() => module.config.ApplyFleetfooted);

        return base.Create(chain);
    }
}
