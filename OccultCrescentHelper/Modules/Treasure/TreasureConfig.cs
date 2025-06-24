using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.Treasure;

[Title("modules.treasure.title")]
public class TreasureConfig : ModuleConfig
{
    [Checkbox]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = true;

    [Checkbox]
    [DependsOn(nameof(Enabled))]
    [Label("modules.treasure.draw.bronze.label")]
    [Tooltip("modules.treasure.draw.bronze.tooltip")]
    public bool DrawLineToBronzeChests { get; set; } = true;

    public bool ShouldDrawLineToBronzeChests => IsPropertyEnabled(nameof(DrawLineToBronzeChests));

    [Checkbox]
    [DependsOn(nameof(Enabled))]
    [Label("modules.treasure.draw.silver.label")]
    [Tooltip("modules.treasure.draw.silver.tooltip")]
    public bool DrawLineToSilverChests { get; set; } = true;

    public bool ShouldDrawLineToSilverChests => IsPropertyEnabled(nameof(DrawLineToSilverChests));

    [Checkbox]
    [Experimental]
    [Illegal]
    [RequiredPlugin("vnavmesh", "Lifestream")]
    [DependsOn(nameof(Enabled))]
    [Label("modules.treasure.hunt.show_button.label")]
    public bool EnableTreasureHunt { get; set; } = false;

    public bool ShouldEnableTreasureHunt => IsPropertyEnabled(nameof(EnableTreasureHunt));

    [FloatRange(10f, 100f)]
    [DependsOn(nameof(Enabled), nameof(EnableTreasureHunt))]
    [Label("modules.treasure.hunt.detection.label")]
    [Tooltip("modules.treasure.hunt.detection.tooltip")]
    public float ChestDetectionRange { get; set; } = 75f;

    [IntRange(1, 28)]
    [Experimental]
    [DependsOn(nameof(Enabled), nameof(EnableTreasureHunt))]
    [Label("modules.treasure.hunt.max_level.label")]
    [Tooltip("modules.treasure.hunt.max_level.tooltip")]
    public int MaxLevel { get; set; } = 28;
}
