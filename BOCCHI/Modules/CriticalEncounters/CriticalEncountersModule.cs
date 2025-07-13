using System.Collections.Generic;
using BOCCHI.Data;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Modules;

namespace BOCCHI.Modules.CriticalEncounters;

[OcelotModule(1002, 6)]
public class CriticalEncountersModule : Module<Plugin, Config>
{
    public override CriticalEncountersConfig Config
    {
        get => PluginConfig.CriticalEncountersConfig;
    }

    public override bool IsEnabled
    {
        get => Config.IsPropertyEnabled(nameof(Config.Enabled));
    }

    public override bool ShouldUpdate
    {
        get => true;
    }

    public readonly CriticalEncounterTracker Tracker;

    public Dictionary<uint, DynamicEvent> CriticalEncounters
    {
        get => Tracker.CriticalEncounters;
    }

    public Dictionary<uint, EventProgress> Progress
    {
        get => Tracker.Progress;
    }

    private readonly Panel panel = new();

    private readonly Alerter alerter;

    public CriticalEncountersModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        Tracker = new CriticalEncounterTracker(this);
        alerter = new Alerter(this);
    }

    public override void Update(IFramework framework)
    {
        Tracker.Tick(framework);
    }

    public override bool RenderMainUi()
    {
        panel.Draw(this);
        return true;
    }

    public override void OnTerritoryChanged(ushort id)
    {
        CriticalEncounters.Clear();
    }

    public override void Dispose()
    {
        base.Dispose();
        alerter.Dispose();
    }
}
