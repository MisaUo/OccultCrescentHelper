using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Ocelot.Modules;

namespace BOCCHI.Modules.Exp;

[OcelotModule(6, 4)]
public class ExpModule : Module<Plugin, Config>
{
    private readonly Panel panel = new();

    public readonly ExpTracker tracker = new();

    public ExpModule(Plugin plugin, Config config)
        : base(plugin, config) { }

    public override ExpConfig config => _config.ExpConfig;

    public override bool enabled => config.IsPropertyEnabled(nameof(config.Enabled));


    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }

    public override void OnChatMessage(
        XivChatType type, int timestamp, SeString sender, SeString message, bool isHandled)
    {
        tracker.OnChatMessage(type, timestamp, sender, message, isHandled);
    }

    public override void OnTerritoryChanged(ushort id)
    {
        tracker.OnTerritoryChange(id);
    }
}
