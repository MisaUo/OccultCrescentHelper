using Ocelot.States;

namespace BOCCHI.Modules.StateManager.States;

[StateAttribute<State>(State.Idle)]
public class IdleHandler : BaseHandler
{
    public override State? Handle(StateManagerModule module)
    {
        if (IsInCombat())
        {
            return State.InCombat;
        }

        if (IsInFate())
        {
            return State.InFate;
        }

        if (IsInCriticalEncounter())
        {
            return State.InCriticalEncounter;
        }

        return null;
    }
}
