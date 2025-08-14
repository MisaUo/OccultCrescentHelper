using BOCCHI.Data;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.States;

namespace BOCCHI.Modules.StateManager.States;

public abstract class BaseHandler(StateManagerModule module) : StateHandler<State, StateManagerModule>(module)
{
    protected bool IsInCombat()
    {
        return Svc.Condition[ConditionFlag.InCombat];
    }

    protected unsafe bool IsInFate()
    {
        return FateManager.Instance()->CurrentFate is not null;
    }

    protected unsafe bool IsInCriticalEncounter()
    {
        return Player.Status.Has(PlayerStatus.HoofingIt) || DynamicEventContainer.GetInstance()->CurrentEventId != 0;
    }
}
