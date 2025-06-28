using Ocelot.Windows;

namespace BOCCHI.Windows;

[OcelotConfigWindow]
public class ConfigWindow(Plugin plugin, Config config) : OcelotConfigWindow(plugin, config);
