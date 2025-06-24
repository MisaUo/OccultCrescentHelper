using System.Linq;
using BOCCHI.Data;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Buff.Chains;

public class BuffChain : ChainFactory
{
    private readonly uint action;
    private readonly Job job;

    private readonly PlayerStatus status;

    public BuffChain(Job job, PlayerStatus status, uint action)
    {
        this.job = job;
        this.status = status;
        this.action = action;
    }

    protected override Chain Create(Chain chain)
    {
        chain
            .Then(_ => PublicContentOccultCrescent.ChangeSupportJob((byte)job.id))
            .WaitUntilStatus((uint)job.status)
            .WaitGcd()
            .UseAction(ActionType.GeneralAction, action)
            .Then(new TaskManagerTask(
                      () => Svc.ClientState.LocalPlayer?.StatusList.Any(s => s.StatusId == (uint)status &&
                                                                             s.RemainingTime >= 1780) == true,
                      new TaskManagerConfiguration { TimeLimitMS = 3000 }))
            .WaitGcd();

        return chain;
    }
}
