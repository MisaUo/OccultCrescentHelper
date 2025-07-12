using System.Numerics;
using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.ForkedTower;

[Text("config.no_traps")]
public class ForkedTowerConfig : ModuleConfig
{
    [Checkbox]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = true;

    [Checkbox] public bool DrawPotentialTrapPositions { get; set; } = true;

    [FloatRange(20f, 300f)]
    [RangeIndicator]
    public float TrapDrawRange { get; set; } = 150f;

    [Color4] public Vector4 TrapDrawColor { get; set; } = Vector4.One;

    [Color4] public Vector4 BigTrapDrawColor { get; set; } = Vector4.One;
}
