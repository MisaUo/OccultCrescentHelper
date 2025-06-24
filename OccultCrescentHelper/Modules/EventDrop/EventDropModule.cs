using Ocelot.Modules;

namespace BOCCHI.Modules.EventDrop;

[OcelotModule(2)]
public class EventDropModule : Module<Plugin, Config>
{
    public EventDropModule(Plugin plugin, Config config)
        : base(plugin, config) { }

    public override EventDropConfig config => _config.EventDropConfig;
}
