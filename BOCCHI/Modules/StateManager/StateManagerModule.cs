using System;
using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.StateManager;

[OcelotModule(6, -1)]
public class StateManagerModule : Module<Plugin, Config>
{
    public override StateManagerConfig config
    {
        get => _config.StateManagerConfig;
    }

    private readonly Panel panel = new();

    private readonly StateManager state = new();

    public event Action? OnEnterIdle
    {
        add => state.OnEnterIdle += value;
        remove => state.OnEnterIdle -= value;
    }

    public event Action? OnExitIdle
    {
        add => state.OnExitIdle += value;
        remove => state.OnExitIdle -= value;
    }

    public event Action? OnEnterInCombat
    {
        add => state.OnEnterInCombat += value;
        remove => state.OnEnterInCombat -= value;
    }

    public event Action? OnExitInCombat
    {
        add => state.OnExitInCombat += value;
        remove => state.OnExitInCombat -= value;
    }

    public event Action? OnEnterInFate
    {
        add => state.OnEnterInFate += value;
        remove => state.OnEnterInFate -= value;
    }

    public event Action? OnExitInFate
    {
        add => state.OnExitInFate += value;
        remove => state.OnExitInFate -= value;
    }

    public event Action? OnEnterInCriticalEncounter
    {
        add => state.OnEnterInCriticalEncounter += value;
        remove => state.OnEnterInCriticalEncounter -= value;
    }

    public event Action? OnExitInCriticalEncounter
    {
        add => state.OnExitInCriticalEncounter += value;
        remove => state.OnExitInCriticalEncounter -= value;
    }

    public StateManagerModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
    }

    public override void Tick(IFramework framework)
    {
        state.Tick(framework);
    }

    public override bool DrawMainUi()
    {
        return panel.Draw(this);
    }

    public State GetState()
    {
        return state.GetState();
    }

    public string GetStateText()
    {
        return state.GetState().ToString();
    }
}
