using System.Collections.Generic;
using System.Numerics;
using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.Treasure;

[OcelotModule(3, 1)]
public class TreasureModule : Module<Plugin, Config>
{
    public override TreasureConfig config {
        get => _config.TreasureConfig;
    }

    public override bool enabled {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    public static Vector4 bronze = new(0.804f, 0.498f, 0.196f, 1f);

    public static Vector4 silver = new(0.753f, 0.753f, 0.753f, 1f);

    public static Vector4 unknown = new(0.6f, 0.2f, 0.8f, 1f);

    private readonly TreasureTracker tracker = new();

    private readonly TreasureHunt hunter = new();

    public List<Treasure> treasures {
        get => tracker.treasures;
    }

    private readonly Panel panel = new();

    private readonly Radar radar = new();

    public TreasureModule(Plugin plugin, Config config)
        : base(plugin, config) { }

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

        if (config.ShouldEnableTreasureHunt)
        {
            hunter.Draw(this);
        }

        return true;
    }
}
