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
}
