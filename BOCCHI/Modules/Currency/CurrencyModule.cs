using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.Currency;

[OcelotModule(int.MaxValue - 1001, 3)]
public class CurrencyModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override CurrencyConfig config
    {
        get => _config.CurrencyConfig;
    }

    public override bool render
    {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    public override bool tick
    {
        get => true;
    }

    public readonly CurrencyTracker Tracker = new();

    private readonly Panel panel = new();

    public override void Tick(IFramework framework)
    {
        Tracker.Tick(framework);
    }

    public override void OnTerritoryChanged(ushort _)
    {
        Tracker.Reset();
    }

    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }
}
