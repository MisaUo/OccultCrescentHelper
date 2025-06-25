using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
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

    private Dictionary<State, Action> handlers;

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
        if (Svc.ClientState.LocalPlayer?.IsDead ?? true)
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
            return;
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
            return;
        }
    }

    public void HandleInFate()
    {
        if (!IsInFate())
        {
            ChangeState(IsInCombat() ? State.InCombat : State.Idle);
            return;
        }
    }

    public void HandleInCriticalEncounter()
    {
        if (!IsInCriticalEncounter())
        {
            ChangeState(IsInCombat() ? State.InCriticalEncounter : State.Idle);
            return;
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

    // 1778 = Hoofing It (Unable to mount)
    private bool IsInCriticalEncounter()
    {
        return Svc.ClientState.LocalPlayer?.StatusList.Any(s => s.StatusId == 1778) ?? false;
    }

    public State GetState()
    {
        return state;
    }
}
