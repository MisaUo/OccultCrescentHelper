using System.Collections.Generic;
using System.Numerics;
using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.Treasure;

[OcelotModule(1003, 1)]
public class TreasureModule(Plugin _plugin, Config config) : Module<Plugin, Config>(_plugin, config)
{
    public override TreasureConfig Config
    {
        get => PluginConfig.TreasureConfig;
    }

    public override bool IsEnabled
    {
        get => Config.IsPropertyEnabled(nameof(Config.Enabled));
    }

    public readonly static Vector4 Bronze = new(0.804f, 0.498f, 0.196f, 1f);

    public readonly static Vector4 Silver = new(0.753f, 0.753f, 0.753f, 1f);

    public readonly static Vector4 Unknown = new(0.6f, 0.2f, 0.8f, 1f);

    public readonly TreasureTracker Tracker = new();

    private TreasureHunt hunter = null!;

    public List<Treasure> Treasures
    {
        get => Tracker.Treasures;
    }

    private readonly Panel panel = new();

    private readonly Radar radar = new();

    public override void PostInitialize()
    {
        hunter = new TreasureHunt(this);
    }

    public override void Update(IFramework framework)
    {
        Tracker.Tick(framework, Plugin);
        hunter.Tick(this);
    }

    public override void Render()
    {
        radar.Draw(this);
    }

    public override bool RenderMainUi()
    {
        panel.Draw(this);

        if (Config.ShouldEnableTreasureHunt)
        {
            hunter.Draw(this);
        }

        return true;
    }
}
