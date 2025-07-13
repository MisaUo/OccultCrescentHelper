using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.Currency;

[OcelotModule(int.MaxValue - 1001, 3)]
public class CurrencyModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override CurrencyConfig Config
    {
        get => PluginConfig.CurrencyConfig;
    }

    public override bool ShouldRender
    {
        get => Config.IsPropertyEnabled(nameof(Config.Enabled));
    }

    public override bool ShouldUpdate
    {
        get => true;
    }

    public readonly CurrencyTracker Tracker = new();

    private readonly Panel panel = new();

    public override void Update(IFramework framework)
    {
        Tracker.Tick(framework);
    }

    public override void OnTerritoryChanged(ushort _)
    {
        Tracker.Reset();
    }

    public override bool RenderMainUi()
    {
        panel.Draw(this);
        return true;
    }
}
