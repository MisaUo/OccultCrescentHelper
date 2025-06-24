using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Buff.Chains;

public unsafe class AllBuffsChain : ChainFactory
{
    private readonly BuffModule module;

    private readonly byte startingJobId;

    public AllBuffsChain(BuffModule module)
    {
        this.module = module;
        startingJobId = PublicContentOccultCrescent.GetState()->CurrentSupportJob;
    }

    protected override Chain Create(Chain chain)
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
