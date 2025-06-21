using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using OccultCrescentHelper.Data;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace OccultCrescentHelper.Modules.Mount.Chains;

public class MountChain : RetryChainFactory
{
    private MountConfig config;

    public MountChain(MountConfig config)
    {
        this.config = config;
    }

    protected unsafe override Chain Create(Chain chain)
    {
        return chain
            .BreakIf(Breaker)
            .ConditionalThen(_ => !config.MountRoulette, _ => ActionManager.Instance()->UseAction(ActionType.Mount, config.Mount))
            // Mount Roulette
            .ConditionalThen(_ => config.MountRoulette, _ => ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9));
    }

    private bool Breaker()
    {
        var player = Svc.ClientState.LocalPlayer;
        if (player == null)
        {
            return true;
        }

        return Svc.Condition[ConditionFlag.Mounted]
            || Svc.Condition[ConditionFlag.BetweenAreas]
            || Svc.Condition[ConditionFlag.BetweenAreas51]
            || Svc.Condition[ConditionFlag.InCombat]
            || player.StatusList.Has(PlayerStatus.HoofingIt)
            || player.IsCasting
            || player.IsDead;
    }

    public override int GetThrottle() => 2000;

    public override bool IsComplete() => Svc.Condition[ConditionFlag.Mounted];
}
