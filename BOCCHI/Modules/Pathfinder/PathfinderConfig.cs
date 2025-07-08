using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.Pathfinder;

[Title]
[Text("config.text")]
public class PathfinderConfig : ModuleConfig
{
    [FloatRange(50f, 500f)] public float ReturnCost { get; set; } = 300f;

    [FloatRange(10f, 500f)] public float TeleportCost { get; set; } = 50f;

    [FloatRange(10f, 100f)] public float DetectionRange { get; set; } = 75f;

    [IntRange(1, 28)] [Experimental] public int MaxLevel { get; set; } = 23;
}
