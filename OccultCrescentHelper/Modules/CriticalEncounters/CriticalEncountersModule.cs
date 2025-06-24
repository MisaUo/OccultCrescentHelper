using System.Collections.Generic;
using BOCCHI.Data;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Modules;

namespace BOCCHI.Modules.CriticalEncounters;

[OcelotModule(8, 6)]
public class CriticalEncountersModule : Module<Plugin, Config>
{
    private readonly Panel panel = new();

    public readonly CriticalEncounterTracker tracker = new();

    public CriticalEncountersModule(Plugin plugin, Config config)
        : base(plugin, config) { }

    public override CriticalEncountersConfig config => _config.CriticalEncountersConfig;

    public override bool enabled => config.IsPropertyEnabled(nameof(config.Enabled));

    public Dictionary<uint, DynamicEvent> criticalEncounters => tracker.criticalEncounters;

    public Dictionary<uint, EventProgress> progress => tracker.progress;

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
