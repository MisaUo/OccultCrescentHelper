using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.Target;

[OcelotModule]
public class TargetModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override void Update(IFramework _)
    {
        TargetHelper.Update();
    }
}
