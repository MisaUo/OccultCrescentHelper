using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using OccultCrescentHelper.Data;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.CriticalEncounters;

[OcelotModule(8, 6)]
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

    public readonly CriticalEncounterTracker tracker = new();

    public Dictionary<uint, DynamicEvent> criticalEncounters
    {
        get => tracker.criticalEncounters;
    }

    public Dictionary<uint, EventProgress> progress
    {
        get => tracker.progress;
    }

    private Panel panel = new();

    public CriticalEncountersModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
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
}
