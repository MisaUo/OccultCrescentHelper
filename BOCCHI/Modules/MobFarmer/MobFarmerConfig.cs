using BOCCHI.Data;
using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.MobFarmer;

[Title]
public class MobFarmerConfig : ModuleConfig
{
    public override string ProviderNamespace
    {
        get => GetType().Namespace!;
    }

    [Enum(typeof(Mob), nameof(MobProvider))]
    public Mob Mob { get; set; } = Mob.Foper;

    [Checkbox] public bool ConsiderSpecialMobs { get; set; } = false;

    [IntRange(1, 28)] public int MaxMobLevel { get; set; } = 28;

    [Checkbox] public bool RenderDebugLines { get; set; } = false;

    [Checkbox]
    [DependsOn(nameof(RenderDebugLines))]
    public bool RenderDebugLinesWhileNotRunning { get; set; } = false;

    public bool ShouldRenderDebugLinesWhileNotRunning
    {
        get => IsPropertyEnabled(nameof(RenderDebugLinesWhileNotRunning));
    }

    [Checkbox] public bool ApplyBattleBell { get; set; } = false;

    [FloatRange(0f, 30f)]
    [DependsOn(nameof(ApplyBattleBell))]
    public float MaximumBattleBellWaitTime { get; set; } = 10f;

    [IntRange(0, 20)] public int MinimumMobsToStartLoop { get; set; } = 0;

    [IntRange(1, 20)] public int MinimumMobsToStartFight { get; set; } = 5;
}
