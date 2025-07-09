using BOCCHI.ActionHelpers;
using BOCCHI.Data;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Buff.Chains;

public class BardBuffChain(BuffModule module) : BuffChain(Job.Bard, PlayerStatus.RomeosBallad, Actions.Bard.RomeosBallad)
{
    protected override Chain Create(Chain chain)
    {
        chain.RunIf(() => module.config.ApplyRomeosBallad);

        return base.Create(chain);
    }
}
