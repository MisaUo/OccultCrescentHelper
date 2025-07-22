using Ocelot.States;

namespace BOCCHI.Modules.StateManager.States;

[StateAttribute<State>(State.InFate)]
public class InFateHandler : BaseHandler
{
    public override State? Handle(StateManagerModule module)
    {
        if (!IsInFate())
        {
            return IsInCombat() ? State.InCombat : State.Idle;
        }

        return null;
    }
}
