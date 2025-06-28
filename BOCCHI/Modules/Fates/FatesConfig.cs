using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.Fates;

[Title("modules.fates.title")]
public class FatesConfig : ModuleConfig
{
    [Checkbox]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = true;
}
