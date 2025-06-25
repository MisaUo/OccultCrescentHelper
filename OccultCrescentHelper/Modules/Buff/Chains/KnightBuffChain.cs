using BOCCHI.Data;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Buff.Chains;

public class KnightBuffChain : BuffChain
{
    private BuffModule module;

    public KnightBuffChain(BuffModule module)
        : base(Job.Knight, PlayerStatus.EnduringFortitude, 32)
    {
        this.module = module;
    }

    protected override Chain Create(Chain chain)
    {
        chain.RunIf(() => module.config.ApplyEnduringFortitude);

        return base.Create(chain);
    }
}
