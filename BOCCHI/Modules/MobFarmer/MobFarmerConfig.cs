using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.MobFarmer;

[Title]
public class MobFarmerConfig : ModuleConfig
{
    // @todo: Mob selector :)

    [Checkbox] public bool ApplyBattleBell { get; set; } = false;
}
