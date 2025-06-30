using System.Collections.Generic;
using BOCCHI.Data;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.Fates;

[OcelotModule(7, 5)]
public class FatesModule : Module<Plugin, Config>
{
    public override FatesConfig config
    {
        get => _config.FatesConfig;
    }

    public override bool enabled
    {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    public override bool tick
    {
        get => true;
    }

    public readonly FateTracker tracker = new();

    public Dictionary<uint, IFate> fates
    {
        get => tracker.fates;
    }

    public Dictionary<uint, EventProgress> progress
    {
        get => tracker.progress;
    }

    private Panel panel = new();

    private Alerter alerter;

    public FatesModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
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

    public override void Dispose()
    {
        base.Dispose();
        alerter.Dispose();
    }
}
