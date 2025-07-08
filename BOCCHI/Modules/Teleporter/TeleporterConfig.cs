using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.Teleporter;

[Title]
public class TeleporterConfig : ModuleConfig
{
    public override string ProviderNamespace
    {
        get => GetType().Namespace!;
    }

    [Checkbox]
    [RequiredPlugin("Lifestream")]
    [IllegalModeCompatible]

    public bool ShouldMount { get; set; } = true;

    [Checkbox]
    [Illegal]
    [RequiredPlugin("vnavmesh")]

    public bool PathToDestination { get; set; } = false;

    [Checkbox] public bool ReturnAfterFate { get; set; } = false;

    [Checkbox] public bool ReturnAfterCriticalEncounter { get; set; } = false;

    [Checkbox]
    [RequiredPlugin("vnavmesh")]

    public bool ApproachAetheryte { get; set; } = false;
}
