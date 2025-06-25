using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.EventDrop;

[OcelotModule(2)]
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
