using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.Carrots;

[OcelotModule(1004, 2)]
public class CarrotsModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override CarrotsConfig Config
    {
        get => PluginConfig.CarrotsConfig;
    }

    public override bool ShouldUpdate
    {
        get => true;
    }

    public override bool IsEnabled
    {
        get => Config.IsPropertyEnabled(nameof(Config.Enabled));
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

    public override void Update(IFramework framework)
    {
        tracker.Tick(framework);
        hunter.Tick(this);
    }

    public override void Render()
    {
        radar.Draw(this);
    }

    public override bool RenderMainUi()
    {
        panel.Draw(this);

        if (Config.ShouldEnableCarrotHunt)
        {
            hunter.Draw(this);
        }

        return true;
    }
}
