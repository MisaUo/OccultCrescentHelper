using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.Carrots;

[OcelotModule(1004, 2)]
public class CarrotsModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
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

    private CarrotHunt hunter = null!;

    public List<Carrot> carrots
    {
        get => tracker.carrots;
    }

    private readonly Panel panel = new();

    private readonly Radar radar = new();

    public override void PostInitialize()
    {
        hunter = new CarrotHunt(this);
    }

    public override void Tick(IFramework framework)
    {
        tracker.Tick(framework, plugin);
        hunter.Tick(this);
    }

    public override void Draw()
    {
        radar.Draw(this);
    }

    public override bool DrawMainUi()
    {
        panel.Draw(this);

        if (config.ShouldEnableCarrotHunt)
        {
            hunter.Draw(this);
        }

        return true;
    }
}
