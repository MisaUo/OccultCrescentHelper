using BOCCHI.Data;
using ECommons.Automation.NeoTaskManager;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Buff.Chains;

public class AllBuffsChain(BuffModule module) : ChainFactory
{
    private readonly Job startingJob = Job.Current;

    protected override Chain Create(Chain chain)
    {
        chain
            .Then(new KnightBuffChain(module))
            .Then(new MonkBuffChain(module))
            .Then(new BardBuffChain(module))
            .Then(_ => startingJob.ChangeTo())
            .WaitUntilStatus(startingJob.UintStatus);

        return chain;
    }

    public override TaskManagerConfiguration Config()
    {
        return new TaskManagerConfiguration { TimeLimitMS = 60000 };
    }
}
