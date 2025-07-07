using System.Collections.Generic;
using BOCCHI.Enums;

namespace BOCCHI.Modules.Treasure;

public struct ToTreasure(uint id, float distance)
{
    public uint Id { get; set; } = id;

    public float Distance { get; set; } = distance;
}

public struct ToAethernet(Aethernet aethernet, float distance)
{
    public Aethernet Aethernet { get; set; } = aethernet;

    public float Distance { get; set; } = distance;
}

public struct TreasureDataSchema()
{
    public Dictionary<uint, List<ToTreasure>> TreasureToTreasureDistances { get; set; } = [];

    public Dictionary<Aethernet, List<ToTreasure>> AethernetToTreasureDistances { get; set; } = [];

    public Dictionary<uint, List<ToAethernet>> TreasureToAethernetDistances { get; set; } = [];
}
