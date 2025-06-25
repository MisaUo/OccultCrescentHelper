using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OccultCrescentHelper.Enums;

namespace OccultCrescentHelper.Modules.Automator;

public enum NavigationType
{
    WalkToEvent,

    ReturnThenWalkToEvent,

    ReturnThenTeleportToEventshard,

    WalkToClosestShardAndTeleportToEventShardThenWalkToEvent,
}

public static class SmartNavigation
{
    private const float RETURN_BASE_COST = 75f;

    public static NavigationType Decide(Vector3 playerPosition, Vector3 destination, AethernetData closestToDestination)
    {
        var closestToPlayer = AethernetData.GetClosestToPlayer();

        var costToWalkToNearestShard = Vector3.Distance(playerPosition, closestToPlayer.position);
        var costToWalkFromEventShardToEvent = Vector3.Distance(closestToDestination.position, destination);
        var costToWalkToEventDirectly = Vector3.Distance(playerPosition, destination);

        var costToReturnThenWalk = RETURN_BASE_COST + Vector3.Distance(Aethernet.BaseCamp.GetData().position, destination);
        var costToReturnTeleportThenWalk = RETURN_BASE_COST + costToWalkFromEventShardToEvent;
        var costToWalkToShardThenEvent = costToWalkToNearestShard + costToWalkFromEventShardToEvent;

        var costs = new Dictionary<NavigationType, float>
        {
            { NavigationType.WalkToEvent, costToWalkToEventDirectly },
            { NavigationType.ReturnThenWalkToEvent, costToReturnThenWalk },
            { NavigationType.ReturnThenTeleportToEventshard, costToReturnTeleportThenWalk },
            { NavigationType.WalkToClosestShardAndTeleportToEventShardThenWalkToEvent, costToWalkToShardThenEvent },
        };

        return costs.OrderBy(kv => kv.Value).First().Key;
    }
}
