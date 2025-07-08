using Ocelot.Modules;

namespace BOCCHI.Modules.EventDrop;

[OcelotModule(4)]
public class EventDropModule : Module<Plugin, Config>
{
    public override EventDropConfig config
    {
        get => _config.EventDropConfig;
    }

    public EventDropModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
    }
}
