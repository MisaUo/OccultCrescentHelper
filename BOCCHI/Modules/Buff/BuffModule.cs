using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.Buff;

[OcelotModule(1005, 2)]
public class BuffModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override BuffConfig config
    {
        get => _config.BuffConfig;
    }

    public override bool enabled
    {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    public override bool tick
    {
        get => true;
    }

    public readonly BuffManager BuffManager = new();

    private readonly Panel panel = new();

    public override void Tick(IFramework framework)
    {
        BuffManager.Tick(framework, this);
    }

    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }

    public bool ShouldRefreshBuffs()
    {
        return BuffManager.ShouldRefresh(this);
    }
}
