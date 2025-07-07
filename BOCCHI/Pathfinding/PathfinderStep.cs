using BOCCHI.Enums;

namespace BOCCHI.Pathfinding;

public class PathfinderStep
{
    public PathfinderStepType Type;

    public uint NodeId = 0;

    public Aethernet Aethernet = Aethernet.BaseCamp;

    public int Flag = 0;

    public static PathfinderStep WalkToDestination(uint id)
    {
        return new PathfinderStep
        {
            Type = PathfinderStepType.WalkToDestination,
            NodeId = id,
        };
    }

    public static PathfinderStep WalkToAethernet(Aethernet aethernet)
    {
        return new PathfinderStep
        {
            Type = PathfinderStepType.WalkToAethernet,
            Aethernet = aethernet,
        };
    }

    public static PathfinderStep TeleportToAethernet(Aethernet aethernet)
    {
        return new PathfinderStep
        {
            Type = PathfinderStepType.TeleportToAethernet,
            Aethernet = aethernet,
        };
    }

    public static PathfinderStep ReturnToBaseCamp()
    {
        return new PathfinderStep
        {
            Type = PathfinderStepType.ReturnToBaseCamp,
        };
    }
}
