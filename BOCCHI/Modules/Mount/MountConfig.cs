using Ocelot.Config.Attributes;
using Ocelot.Modules;
using ExcelMount = Lumina.Excel.Sheets.Mount;

namespace BOCCHI.Modules.Mount;

[Title]
public class MountConfig : ModuleConfig
{
    public override string ProviderNamespace
    {
        get => GetType().Namespace!;
    }

    [ExcelSheet(typeof(ExcelMount), nameof(MountProvider))]
    [IllegalModeCompatible]

    public uint Mount { get; set; } = 1;

    [Checkbox] [IllegalModeCompatible] public bool MountRoulette { get; set; } = false;
}
