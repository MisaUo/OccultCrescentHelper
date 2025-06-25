using Ocelot.Config.Attributes;
using Ocelot.Modules;
using ExcelMount = Lumina.Excel.Sheets.Mount;

namespace BOCCHI.Modules.Mount;

[Title("modules.mount.title")]
public class MountConfig : ModuleConfig
{
    public override string ProviderNamespace => GetType().Namespace!;

    [ExcelSheet(typeof(ExcelMount), nameof(MountProvider))]
    [IllegalModeCompatible]
    [Label("modules.mount.mount.label")]
    [Tooltip("modules.mount.mount.tooltip")]
    public uint Mount { get; set; } = 1;

    [Checkbox]
    [IllegalModeCompatible]
    [Label("modules.mount.roulette.label")]
    [Tooltip("modules.mount.roulette.tooltip")]
    public bool MountRoulette { get; set; } = false;
}
