using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.Carrots;

[Title("modules.carrots.title")]
public class CarrotsConfig : ModuleConfig
{
    [Checkbox]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = true;

    [Checkbox]
    [DependsOn(nameof(Enabled))]
    [Label("modules.carrots.draw.label")]
    [Tooltip("modules.carrots.draw.tooltip")]
    public bool DrawLineToCarrots { get; set; } = true;

    public bool ShouldDrawLineToCarrots
    {
        get => IsPropertyEnabled(nameof(DrawLineToCarrots));
    }

    // [Checkbox]
    // [Experimental]
    // [Illegal]
    // [RequiredPlugin("vnavmesh", "Lifestream")]
    // [DependsOn(nameof(Enabled))]
    // [Label("modules.carrot.config.hunt.show_button.label")]
    // public bool EnableCarrotHunt { get; set; } = false;
    //
    // public bool ShouldEnableCarrotHunt
    // {
    //     get => IsPropertyEnabled(nameof(EnableCarrotHunt));
    // }
    //
    // [FloatRange(10f, 100f)]
    // [DependsOn(nameof(Enabled), nameof(EnableCarrotHunt))]
    // [Label("modules.carrot.config.hunt.detection.label")]
    // [Tooltip("modules.carrot.config.hunt.detection.tooltip")]
    // public float CarrotDetectionRange { get; set; } = 75f;
    //
    // [IntRange(1, 28)]
    // [Experimental]
    // [DependsOn(nameof(Enabled), nameof(EnableCarrotHunt))]
    // [Label("modules.carrot.config.hunt.max_level.label")]
    // [Tooltip("modules.carrot.config.hunt.max_level.tooltip")]
    // public int MaxLevel { get; set; } = 24;
    //
    // [Checkbox]
    // [Experimental]
    // [DependsOn(nameof(Enabled), nameof(EnableCarrotHunt))]
    // [Label("modules.carrot.config.hunt.repeat.label")]
    // public bool RepeatCarrotHunt { get; set; } = false;
}
