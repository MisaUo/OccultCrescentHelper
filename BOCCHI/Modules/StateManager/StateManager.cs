using System.Collections.Generic;
using BOCCHI.Data;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using Action = System.Action;

namespace BOCCHI.Modules.StateManager;

public class StateManager
{
    private State state = State.Idle;

    public event Action? OnEnterIdle;

    public event Action? OnExitIdle;

    public event Action? OnEnterInCombat;

    public event Action? OnExitInCombat;

    public event Action? OnEnterInFate;

    public event Action? OnExitInFate;

    public event Action? OnEnterInCriticalEncounter;

    public event Action? OnExitInCriticalEncounter;

    private readonly Dictionary<State, Action> handlers;

    public StateManager()
    {
        handlers = new Dictionary<State, Action>
        {
            { State.Idle, HandleIdle },
            { State.InCombat, HandleInCombat },
            { State.InFate, HandleInFate },
            { State.InCriticalEncounter, HandleInCriticalEncounter },
        };
    }

    public void Tick(IFramework _)
    {
        if (Player.IsDead)
        {
            return;
        }

        handlers[state]();
    }


    private void HandleIdle()
    {
        if (IsInCombat())
        {
            ChangeState(State.InCombat);
            return;
        }

        if (IsInFate())
        {
            ChangeState(State.InFate);
            return;
        }

        if (IsInCriticalEncounter())
        {
            ChangeState(State.InCriticalEncounter);
        }
    }

    private void HandleInCombat()
    {
        if (IsInFate())
        {
            ChangeState(State.InFate);
            return;
        }

        if (IsInCriticalEncounter())
        {
            ChangeState(State.InCriticalEncounter);
            return;
        }


        if (!IsInCombat())
        {
            ChangeState(State.Idle);
        }
    }

    public void HandleInFate()
    {
        if (!IsInFate())
        {
            ChangeState(IsInCombat() ? State.InCombat : State.Idle);
        }
    }

    public void HandleInCriticalEncounter()
    {
        if (!IsInCriticalEncounter())
        {
            ChangeState(IsInCombat() ? State.InCriticalEncounter : State.Idle);
        }
    }

    private void ChangeState(State newState)
    {
        if (newState == state)
        {
            return;
        }

        var oldState = state;
        Svc.Log.Info($"[StateManager] State changed from {oldState} to {newState}");

        InvokeExit(oldState);
        state = newState;
        InvokeEnter(newState);
    }

    private void InvokeEnter(State s)
    {
        switch (s)
        {
            case State.Idle: OnEnterIdle?.Invoke(); break;
            case State.InCombat: OnEnterInCombat?.Invoke(); break;
            case State.InFate: OnEnterInFate?.Invoke(); break;
            case State.InCriticalEncounter: OnEnterInCriticalEncounter?.Invoke(); break;
        }
    }

    private void InvokeExit(State s)
    {
        switch (s)
        {
            case State.Idle: OnExitIdle?.Invoke(); break;
            case State.InCombat: OnExitInCombat?.Invoke(); break;
            case State.InFate: OnExitInFate?.Invoke(); break;
            case State.InCriticalEncounter: OnExitInCriticalEncounter?.Invoke(); break;
        }
    }

    private bool IsInCombat()
    {
        return Svc.Condition[ConditionFlag.InCombat];
    }

    private unsafe bool IsInFate()
    {
        return FateManager.Instance()->CurrentFate is not null;
    }

    private bool IsInCriticalEncounter()
    {
        return Player.Status.Has(PlayerStatus.HoofingIt);
    }

    public State GetState()
    {
        return state;
    }
}
