using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Ocelot.Modules;

namespace BOCCHI.Modules.Exp;

[OcelotModule(int.MaxValue - 1000, 4)]
public class ExpModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override ExpConfig config
    {
        get => _config.ExpConfig;
    }

    public override bool enabled
    {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    public readonly ExpTracker tracker = new();

    private readonly Panel panel = new();


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
