
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.Automator;

[OcelotModule]
public class AutomatorModule : Module<Plugin, Config>
{
    public override AutomatorConfig config {
        get => _config.AutomatorConfig;
    }

    public override bool enabled => config.IsPropertyEnabled(nameof(config.Enabled));

    public readonly Automator automator = new();

    public readonly Panel panel = new();

    private List<uint> occultCrescentTerritoryIds = [1252];

    public AutomatorModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        config.AutomatorConfig.Enabled = false;
        config.Save();
    }


    public override void Tick(IFramework framework) => automator.Tick(this, framework);


    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }

    public override void OnTerritoryChanged(ushort id)
    {
        if (occultCrescentTerritoryIds.Contains(id))
        {
            return;
        }

        automator.Refresh();
        config.Enabled = false;
        plugin.config.Save();
    }
}
