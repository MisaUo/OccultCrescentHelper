using System.Collections.Generic;
using BOCCHI.Data;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.Fates;

[OcelotModule(7, 5)]
public class FatesModule : Module<Plugin, Config>
{
    private readonly Panel panel = new();

    public readonly FateTracker tracker = new();

    public FatesModule(Plugin plugin, Config config)
        : base(plugin, config) { }

    public override FatesConfig config => _config.FatesConfig;

    public override bool enabled => config.IsPropertyEnabled(nameof(config.Enabled));

    public Dictionary<uint, IFate> fates => tracker.fates;

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
