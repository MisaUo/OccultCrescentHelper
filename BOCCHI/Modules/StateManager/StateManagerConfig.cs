using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.StateManager;

[Title]
public class StateManagerConfig : ModuleConfig
{
    [Checkbox] public bool ShowDebug { get; set; } = false;
}
