using System.Linq;
using BOCCHI.ActionHelpers;
using BOCCHI.Data;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Buff.Chains;

public abstract class BuffChain(Job job, PlayerStatus buff, Action action) : ChainFactory
{
    protected override Chain Create(Chain chain)
    {
        chain.RunIf(ShouldRun);
        chain.Then(_ => job.ChangeTo()).WaitUntilStatus(job.UintStatus);

        chain = action
            .CastOnChain(chain)
            .Then(_ => Player.Status.Has(buff))
            .Then(_ => Player.Status.Get(buff)?.RemainingTime >= 1780)
            .Debug("Donzo");

        return chain;
    }

    public override TaskManagerConfiguration? Config()
    {
        return new TaskManagerConfiguration { TimeLimitMS = 15000 };
    }

    protected abstract bool ShouldRun();
}
