using System.Numerics;
using BOCCHI.Data;
using OccultCrescentHelper.Chains; // This is in Ocelot...
using Ocelot.Chain;
using Ocelot.IPC;

namespace BOCCHI.Chains;

public class PathfindingChain : ChainFactory
{
    private readonly EventData data;

    private readonly Vector3 destination;

    private readonly float? maxRadius;

    private readonly float? minRadius;

    private readonly bool useCustomPaths;

    private readonly VNavmesh vnav;

    public PathfindingChain(
        VNavmesh vnav,
        Vector3 destination,
        EventData data,
        bool useCustomPaths,
        float? maxRadius = null,
        float? minRadius = null)
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
