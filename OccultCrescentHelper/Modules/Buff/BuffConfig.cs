using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.Buff;

[Title("modules.buff.config.title")]
public class BuffConfig : ModuleConfig
{
    [Checkbox]
    [IllegalModeCompatible]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = true;

    [Checkbox]
    [IllegalModeCompatible]
    [Label("modules.buff.config.romeos_ballad.label")]
    [Tooltip("modules.buff.config.romeos_ballad.tooltip")]
    public bool ApplyRomeosBallad { get; set; } = true;

    [Checkbox]
    [IllegalModeCompatible]
    [Label("modules.buff.config.enduring_fortitude.label")]
    [Tooltip("modules.buff.config.enduring_fortitude.tooltip")]
    public bool ApplyEnduringFortitude { get; set; } = true;

    [Checkbox]
    [IllegalModeCompatible]
    [Label("modules.buff.config.fleetfooted.label")]
    [Tooltip("modules.buff.config.fleetfooted.tooltip")]
    public bool ApplyFleetfooted { get; set; } = true;

    [IntRange(0, 25)]
    [IllegalModeCompatible]
    [Label("modules.buff.config.threshold.label")]
    [Tooltip("modules.buff.config.threshold.tooltip")]
    public int ReapplyThreshold { get; set; } = 10;
}
