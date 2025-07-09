using BOCCHI.ActionHelpers;
using BOCCHI.Data;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Buff.Chains;

public class KnightBuffChain(BuffModule module) : BuffChain(Job.Knight, PlayerStatus.EnduringFortitude, Actions.Knight.Pray)
{
    protected override Chain Create(Chain chain)
    {
        chain.RunIf(() => module.config.ApplyEnduringFortitude);

        return base.Create(chain);
    }
}