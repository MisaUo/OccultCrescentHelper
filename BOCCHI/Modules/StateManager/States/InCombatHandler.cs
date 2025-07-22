using Ocelot.States;

namespace BOCCHI.Modules.StateManager.States;

[StateAttribute<State>(State.InCombat)]
public class InCombatHandler : BaseHandler
{
    public override State? Handle(StateManagerModule module)
    {
        if (IsInFate())
        {
            return State.InFate;
        }

        if (IsInCriticalEncounter())
        {
            return State.InCriticalEncounter;
        }

        if (!IsInCombat())
        {
            return State.Idle;
        }

        return null;
    }
}
