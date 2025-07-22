using Ocelot.States;

namespace BOCCHI.Modules.StateManager.States;

[StateAttribute<State>(State.InCriticalEncounter)]
public class InCriticalEncounterHandler : BaseHandler
{
    public override State? Handle(StateManagerModule module)
    {
        if (!IsInCriticalEncounter())
        {
            return IsInCombat() ? State.InCriticalEncounter : State.Idle;
        }

        return null;
    }
}
