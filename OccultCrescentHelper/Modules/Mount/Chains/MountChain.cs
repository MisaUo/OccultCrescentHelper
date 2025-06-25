using BOCCHI.Data;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.Mount.Chains;

public class MountChain(MountConfig config) : RetryChainFactory
{
    private bool isFirstThrottle = true;

    protected override unsafe Chain Create(Chain chain)
    {
        return chain
               .BreakIf(Breaker)
               .ConditionalThen(_ => !config.MountRoulette,
                                _ => ActionManager.Instance()->UseAction(ActionType.Mount, config.Mount))
               // Mount Roulette
               .ConditionalThen(_ => config.MountRoulette,
                                _ => ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9));
    }

    private static bool Breaker()
    {
        var player = Svc.ClientState.LocalPlayer;
        if (player == null) return true;

        return Svc.Condition[ConditionFlag.Mounted]
               || Svc.Condition[ConditionFlag.BetweenAreas]
               || Svc.Condition[ConditionFlag.BetweenAreas51]
               || Svc.Condition[ConditionFlag.InCombat]
               || player.StatusList.Has(PlayerStatus.HoofingIt)
               || player.IsCasting
               || player.IsDead;
    }

    public override int GetThrottle()
    {
        if (isFirstThrottle)
        {
            isFirstThrottle = false;
            return 500;
        }

        return 5000;
    }

    public override bool IsComplete()
    {
        return Svc.Condition[ConditionFlag.Mounted];
    }
}
