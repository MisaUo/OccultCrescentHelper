using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.Buff;

[OcelotModule(5, 2)]
public class BuffModule : Module<Plugin, Config>
{
    public override BuffConfig config {
        get => _config.BuffConfig;
    }

    public override bool enabled {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    public readonly BuffManager buffs = new();

    private Panel panel = new();

    public BuffModule(Plugin plugin, Config config)
        : base(plugin, config) { }

    public override void Tick(IFramework framework)
    {
        buffs.Tick(framework, this);
    }

    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }

    public bool ShouldRefreshBuffs()
    {
        return buffs.ShouldRefresh(this);
    }
}
