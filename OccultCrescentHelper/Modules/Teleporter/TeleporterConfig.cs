using Lumina.Excel.Sheets;
using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.Teleporter;

[Title("modules.teleporter.title")]
public class TeleporterConfig : ModuleConfig
{
    public override string ProviderNamespace
    {
        get => "OccultCrescentHelper.Modules.Teleporter";
    }

    [Checkbox]
    [RequiredPlugin("Lifestream")]
    [IllegalModeCompatible]
    [Label("modules.teleporter.should_mount.label")]
    [Tooltip("modules.teleporter.should_mount.tooltip")]
    public bool ShouldMount { get; set; } = true;

    [Checkbox]
    [Illegal]
    [RequiredPlugin("vnavmesh")]
    [Label("modules.teleporter.path_to_destination.label")]
    [Tooltip("modules.teleporter.path_to_destination.tooltip")]
    public bool PathToDestination { get; set; } = false;

    [Checkbox]
    [RequiredPlugin("vnavmesh")]
    [DependsOn(nameof(PathToDestination))]
    [Label("modules.teleporter.use_custom_paths.label")]
    [Tooltip("modules.teleporter.use_custom_paths.tooltip")]
    public bool UseCustomPaths { get; set; } = true;

    public bool ShouldUseCustomPaths
    {
        get => IsPropertyEnabled(nameof(UseCustomPaths));
    }

    [Checkbox]
    [Label("modules.teleporter.return_after_fate.label")]
    [Tooltip("modules.teleporter.return_after_fate.tooltip")]
    public bool ReturnAfterFate { get; set; } = false;

    [Checkbox]
    [Label("modules.teleporter.return_after_critical.label")]
    [Tooltip("modules.teleporter.return_after_critical.tooltip")]
    public bool ReturnAfterCritcalEncounter { get; set; } = false;

    [Checkbox]
    [RequiredPlugin("vnavmesh")]
    [Label("modules.teleporter.approach_aetheryte.label")]
    [Tooltip("modules.teleporter.approach_aetheryte.tooltip")]
    public bool ApproachAetheryte { get; set; } = false;
}
