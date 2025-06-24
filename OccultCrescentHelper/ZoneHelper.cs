using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BOCCHI.Enums;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;

namespace BOCCHI;

public static class ZoneHelper
{
    public static Aethernet GetClosestAethernetShard(Vector3 position)
    {
        return AethernetData.All().OrderBy(data => Vector3.Distance(position, data.position)).First()!.aethernet;
    }

    public static IList<IGameObject> GetNearbyAethernetShards(float range = 4.3f)
    {
        var playerPos = Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero;

        return Svc.Objects
                  .Where(o => o != null)
                  .Where(o => o.ObjectKind == ObjectKind.EventObj)
                  .Where(o => AethernetData.All().Select(datum => datum.dataId).Contains(o.DataId))
                  .Where(o => Vector3.Distance(o.Position, playerPos) <= range)
                  .ToList();
    }

    public static bool IsNearAethernetShard(Aethernet aethernet, float range = 4.3f)
    {
        return GetNearbyAethernetShards(range).Where(o => o.DataId == aethernet.GetData().dataId).Count() > 0;
    }

    public static IList<IGameObject> GetNearbyKnowledgeCrystal(float range = 4.5f)
    {
        var playerPos = Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero;

        return Svc.Objects
                  .Where(o => o != null)
                  .Where(o => o.ObjectKind == ObjectKind.EventObj)
                  .Where(o => o.DataId == (uint)OccultObjectType.KnowledgeCrystal)
                  .Where(o => Vector3.Distance(o.Position, playerPos) <= range)
                  .ToList();
    }

    public static bool IsNearKnowledgeCrystal(float range = 4.5f)
    {
        return GetNearbyKnowledgeCrystal(range).Count() > 0;
    }
}
