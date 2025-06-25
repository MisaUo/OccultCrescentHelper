using System.Linq;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using OccultCrescentHelper.Data;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace OccultCrescentHelper.Modules.Buff.Chains;

public class BuffChain : ChainFactory
{
    private Job job;

    private PlayerStatus status;

    private uint action;

    public BuffChain(Job job, PlayerStatus status, uint action)
    {
        this.job = job;
        this.status = status;
        this.action = action;
    }

    protected override unsafe Chain Create(Chain chain)
    {
        chain
            .Then(_ => PublicContentOccultCrescent.ChangeSupportJob((byte)job.id))
            .WaitUntilStatus((uint)job.status)
            .WaitGcd()
            .UseAction(ActionType.GeneralAction, action)
            .Then(new TaskManagerTask(() => Svc.ClientState.LocalPlayer?.StatusList.Any(s => s.StatusId == (uint)status && s.RemainingTime >= 1780) == true,
                new TaskManagerConfiguration { TimeLimitMS = 3000 }))
            .WaitGcd();

        return chain;
    }
}
