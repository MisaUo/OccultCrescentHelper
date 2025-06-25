using System;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.Exp;

[OcelotModule(6, 4)]
public class ExpModule : Module<Plugin, Config>
{
    public override ExpConfig config {
        get => _config.ExpConfig;
    }

    public override bool enabled {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    public readonly ExpTracker tracker = new();

    private Panel panel = new();

    public ExpModule(Plugin plugin, Config config)
        : base(plugin, config) { }


    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }

    public override void OnChatMessage(XivChatType type, int timestamp, SeString sender, SeString message, bool isHandled)
    {
        tracker.OnChatMessage(type, timestamp, sender, message, isHandled);
    }

    public override void OnTerritoryChanged(ushort id)
    {
        tracker.OnTerritoryChange(id);
    }
}
