using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.StateManager;

[Title("modules.state_manager.title")]
public class StateManagerConfig : ModuleConfig
{
    [Checkbox]
    [Label("modules.state_manager.show_debug.label")]
    [Tooltip("modules.state_manager.show_debug.tooltip")]
    public bool ShowDebug { get; set; } = false;
}
