using System.Numerics;
using OccultCrescentHelper.Data;
using Ocelot.Chain;
using Ocelot.IPC;

namespace OccultCrescentHelper.Chains;

public class PathfindingChain : ChainFactory
{
    private VNavmesh vnav;

    private Vector3 destination;

    private EventData data;

    private bool useCustomPaths = false;

    private float? maxRadius = null;

    private float? minRadius = null;

    public PathfindingChain(VNavmesh vnav, Vector3 destination, EventData data, bool useCustomPaths, float? maxRadius = null, float? minRadius = null)
    {
        this.vnav = vnav;
        this.destination = destination;
        this.data = data;
        this.useCustomPaths = useCustomPaths;
        this.maxRadius = maxRadius;
        this.minRadius = minRadius;
    }

    protected override Chain Create(Chain chain)
    {
        if (useCustomPaths && data.pathFactory != null)
        {
            return Chain.Create("Prowler")
                .Then(new ProwlerChain(vnav, data.pathFactory, destination));
        }

        return Chain.Create("Pathfinding")
            .Then(PathfindAndMoveToChain.RandomNearby(vnav, destination, maxRadius ?? 1f, minRadius ?? 0f));
    }
}
