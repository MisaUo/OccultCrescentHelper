using Ocelot.Chain;
using Ocelot.Modules;

namespace BOCCHI.Modules.Mount;

[OcelotModule(int.MinValue)]
public class MountModule : Module<Plugin, Config>
{
    public static ChainQueue MountMaintainer
    {
        get => ChainManager.Get("OCH##MountMaintainer");
    }

    public override MountConfig config
    {
        get => _config.MountConfig;
    }

    public MountModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
    }
}
