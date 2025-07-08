using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.Currency;

[Title]
public class CurrencyConfig : ModuleConfig
{
    [Checkbox]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = true;
}
