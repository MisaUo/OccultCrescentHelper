using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.CriticalEncounters;

[Title("modules.critical_encounters.title")]
public class CriticalEncountersConfig : ModuleConfig
{
    [Checkbox]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = true;
}
