using Ocelot.Modules;

namespace BOCCHI.Modules.Mount;

[OcelotModule(1)]
public class MountModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override MountConfig config
    {
        get => _config.MountConfig;
    }
}
