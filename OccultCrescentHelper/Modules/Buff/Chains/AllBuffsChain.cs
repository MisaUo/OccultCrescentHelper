using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace OccultCrescentHelper.Modules.Buff.Chains;

public unsafe class AllBuffsChain : ChainFactory
{
    private BuffModule module;

    private byte startingJobId;

    public AllBuffsChain(BuffModule module)
    {
        this.module = module;
        startingJobId = PublicContentOccultCrescent.GetState()->CurrentSupportJob;
    }

    protected override unsafe Chain Create(Chain chain)
    {
        chain
            .Then(new KnightBuffChain(module))
            .Then(new MonkBuffChain(module))
            .Then(new BardBuffChain(module))
            .Then(_ => PublicContentOccultCrescent.ChangeSupportJob(startingJobId))
            .WaitGcd();

        return chain;
    }
}
