using Ocelot.Windows;

namespace OccultCrescentHelper.Windows;

[OcelotConfigWindow]
public class ConfigWindow : OcelotConfigWindow
{
    public ConfigWindow(Plugin plugin, Config config)
        : base(plugin, config)
    {
    }
}
