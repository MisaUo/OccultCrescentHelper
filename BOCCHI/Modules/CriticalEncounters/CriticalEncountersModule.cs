using System.Collections.Generic;
using BOCCHI.Data;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Modules;

namespace BOCCHI.Modules.CriticalEncounters;

[OcelotModule(1002, 6)]
public class CriticalEncountersModule : Module<Plugin, Config>
{
    public override CriticalEncountersConfig config
    {
        get => _config.CriticalEncountersConfig;
    }

    public override bool enabled
    {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    public override bool tick
    {
        get => true;
    }

    public readonly CriticalEncounterTracker tracker;

    public Dictionary<uint, DynamicEvent> criticalEncounters
    {
        get => tracker.criticalEncounters;
    }

    public Dictionary<uint, EventProgress> progress
    {
        get => tracker.progress;
    }

    private Panel panel = new();

    private Alerter alerter;

    public CriticalEncountersModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        tracker = new CriticalEncounterTracker(this);
        alerter = new Alerter(this);
    }

    public override void Tick(IFramework framework)
    {
        tracker.Tick(framework);
    }

    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }

    public override void OnTerritoryChanged(ushort id)
    {
        criticalEncounters.Clear();
    }

    public override void Dispose()
    {
        base.Dispose();
        alerter.Dispose();
    }
}
