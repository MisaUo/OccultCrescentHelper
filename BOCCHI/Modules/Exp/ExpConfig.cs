using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.Exp;

[Title]
public class ExpConfig : ModuleConfig
{
    [Checkbox]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = true;
}
