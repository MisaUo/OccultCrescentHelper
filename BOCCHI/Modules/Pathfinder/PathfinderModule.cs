using Ocelot.Modules;

namespace BOCCHI.Modules.Pathfinder;

[OcelotModule(3)]
public class PathfinderModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override PathfinderConfig config
    {
        get => _config.PathfinderConfig;
    }
}
