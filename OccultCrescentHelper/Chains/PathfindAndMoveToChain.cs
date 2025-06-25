using System;
using System.Numerics;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;

namespace OccultCrescentHelper.Chains;

public class PathfindAndMoveToChain : ChainFactory
{
    private VNavmesh vnav;

    private Vector3 destination;

    public PathfindAndMoveToChain(VNavmesh vnav, Vector3 destination, float maxRadius = 1f, float minRadius = 0f)
    {
        this.vnav = vnav;
        this.destination = destination;
    }

    public static PathfindAndMoveToChain RandomNearby(VNavmesh vnav, Vector3 destination, float maxRadius = 1f, float minRadius = 0f)
    {
        var angle = (float)(Random.Shared.NextDouble() * MathF.Tau);
        var distance = minRadius + (float)(Random.Shared.NextDouble() * (maxRadius - minRadius));

        var offsetX = MathF.Cos(angle) * distance;
        var offsetZ = MathF.Sin(angle) * distance;

        destination = new Vector3(destination.X + offsetX, destination.Y, destination.Z + offsetZ);
        Svc.Log.Info($"Pre: {destination.X:f2}, {destination.Y:f2}, {destination.Z:f2}");
        destination = vnav.FindPointOnFloor(destination, false, 0.5f) ?? destination;
        Svc.Log.Info($"Post: {destination.X:f2}, {destination.Y:f2}, {destination.Z:f2}");

        return new PathfindAndMoveToChain(vnav, destination);
    }


    protected override Chain Create(Chain chain)
    {
        return chain
            .PathfindAndMoveTo(vnav, destination);
    }

    public override TaskManagerConfiguration? Config()
    {
        return new TaskManagerConfiguration {
            TimeLimitMS = 180000,
        };
    }
}
