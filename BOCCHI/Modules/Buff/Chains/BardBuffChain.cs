using BOCCHI.Data;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Buff.Chains;

public class BardBuffChain : BuffChain
{
    private readonly BuffModule module;

    public BardBuffChain(BuffModule module)
        : base(Job.Bard, PlayerStatus.RomeosBallad, 32)
    {
        this.module = module;
    }

    protected override Chain Create(Chain chain)
    {
        chain.RunIf(() => module.config.ApplyRomeosBallad);

        return base.Create(chain);
    }
}
