using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.InstanceIdentifier;

[Title("modules.instance_identifier.title")]
public class InstanceIdentifierConfig : ModuleConfig
{
    [Checkbox]
    [Label("modules.instance_identifier.show.label")]
    [Tooltip("modules.instance_identifier.show.tooltip")]
    public bool Show { get; set; } = true;
}
