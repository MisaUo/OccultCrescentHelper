using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.WindowManager;

[Title("modules.window_manager.title")]
public class WindowManagerConfig : ModuleConfig
{
    [Checkbox]
    [Label("modules.window_manager.main_on_load.label")]
    [Tooltip("modules.window_manager.main_on_load.tooltip")]
    public bool OpenMainOnStartUp { get; set; } = false;

    [Checkbox]
    [Label("modules.window_manager.main_on_enter.label")]
    [Tooltip("modules.window_manager.main_on_enter.tooltip")]
    public bool OpenMainOnEnter { get; set; } = true;

    [Checkbox]
    [Label("modules.window_manager.main_on_exit.label")]
    [Tooltip("modules.window_manager.main_on_exit.tooltip")]
    public bool CloseMainOnExit { get; set; } = true;

    [Checkbox]
    [Label("modules.window_manager.main_in_combat.label")]
    [Tooltip("modules.window_manager.main_in_combat.tooltip")]
    public bool HideMainInCombat { get; set; } = false;

    [Checkbox]
    [Label("modules.window_manager.config_on_load.label")]
    [Tooltip("modules.window_manager.config_on_load.tooltip")]
    public bool OpenConfigOnStartUp { get; set; } = false;

    [Checkbox]
    [Label("modules.window_manager.config_on_enter.label")]
    [Tooltip("modules.window_manager.config_on_enter.tooltip")]
    public bool OpenConfigOnEnter { get; set; } = false;

    [Checkbox]
    [Label("modules.window_manager.config_on_exit.label")]
    [Tooltip("modules.window_manager.config_on_exit.tooltip")]
    public bool CloseConfigOnExit { get; set; } = true;

    [Checkbox]
    [Label("modules.window_manager.config_in_combat.label")]
    [Tooltip("modules.window_manager.config_in_combat.tooltip")]
    public bool HideConfigInCombat { get; set; } = false;
}
