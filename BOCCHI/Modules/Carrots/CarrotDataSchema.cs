using System.Collections.Generic;
using BOCCHI.Enums;
using BOCCHI.Modules.Treasure;

namespace BOCCHI.Modules.Carrots;

public struct ToCarrot(uint id, float distance)
{
    public uint Id { get; set; } = id;

    public float Distance { get; set; } = distance;
}

public struct CarrotDataSchema()
{
    public Dictionary<uint, List<ToCarrot>> CarrotToCarrotDistances { get; set; } = [];

    public Dictionary<Aethernet, List<ToCarrot>> AethernetToCarrotDistances { get; set; } = [];

    public Dictionary<uint, List<ToAethernet>> CarrotsToAethernetDistances { get; set; } = [];
}
