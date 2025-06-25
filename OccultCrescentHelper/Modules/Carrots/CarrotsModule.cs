using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.Carrots;

[OcelotModule(4, 2)]
public class CarrotsModule : Module<Plugin, Config>
{
    private readonly Panel panel = new();

    private readonly Radar radar = new();

    private readonly CarrotsTracker tracker = new();

    public CarrotsModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
    }

    public override CarrotsConfig config
    {
        get => _config.CarrotsConfig;
    }

    public override bool enabled
    {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    public List<Carrot> carrots
    {
        get => tracker.carrots;
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
