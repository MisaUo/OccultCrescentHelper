using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.Carrots;

[OcelotModule(4, 2)]
public class CarrotsModule : Module<Plugin, Config>
{
    public override CarrotsConfig config
    {
        get => _config.CarrotsConfig;
    }

    public override bool enabled
    {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    private readonly CarrotsTracker tracker = new();

    public List<Carrot> carrots
    {
        get => tracker.carrots;
    }

    private readonly Panel panel = new();

    private readonly Radar radar = new();

    public CarrotsModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
    }

    public override void Tick(IFramework framework)
    {
        tracker.Tick(framework, plugin);
    }

    public override void Draw()
    {
        radar.Draw(this);
    }

    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }
}
