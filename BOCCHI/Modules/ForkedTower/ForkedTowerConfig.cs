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

    [Checkbox] public bool DrawPotentialTrapPositions { get; set; } = false;

    [Checkbox] public bool DrawSimpleMode { get; set; } = false;

    [Checkbox] public bool DrawOutlineForComplexMode { get; set; } = false;


    [FloatRange(20f, 300f)]
    [RangeIndicator]
    public float TrapDrawRange { get; set; } = 150f;

    [Color4] public Vector4 TrapDrawColor { get; set; } = Vector4.One;

    [Color4] public Vector4 BigTrapDrawColor { get; set; } = Vector4.One;

    [Checkbox] [Experimental] public bool DrawSmallTrapRange { get; set; } = false;

    [Checkbox] [Experimental] public bool DrawBigTrapRange { get; set; } = false;

    [Checkbox] [Experimental] public bool StopRenderingCompleteGroups { get; set; } = false;
}
