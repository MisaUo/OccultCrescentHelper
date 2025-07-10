using Ocelot.Modules;

namespace BOCCHI.Modules.EventDrop;

[OcelotModule(4)]
public class EventDropModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override EventDropConfig config
    {
        get => _config.EventDropConfig;
    }
}
