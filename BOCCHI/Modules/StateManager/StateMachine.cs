﻿using ECommons.GameHelpers;
using Ocelot.States;

namespace BOCCHI.Modules.StateManager;

public class StateMachine(State state, StateManagerModule module) : StateMachine<State, StateManagerModule>(state, module)
{
    protected override bool ShouldUpdate()
    {
        return Player.Available && !Player.IsDead;
    }
}
