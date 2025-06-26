using BOCCHI.Data;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Buff.Chains;

public class MonkBuffChain : BuffChain
{
    private readonly BuffModule module;

    public MonkBuffChain(BuffModule module)
        : base(Job.Monk, PlayerStatus.Fleetfooted, 33)
    {
        this.module = module;
    }

    protected override Chain Create(Chain chain)
    {
        chain.RunIf(() => module.config.ApplyFleetfooted);

        return base.Create(chain);
    }
}
