using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.Data;

[Title("modules.data.title")]
[Text("modules.data.explanation")]
public class DataConfig : ModuleConfig
{
    [Checkbox]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = false;
}
