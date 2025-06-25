using System.Collections.Generic;
using System.Numerics;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;

namespace BOCCHI.Chains;

public class FollowPathChain : ChainFactory
{
    private readonly List<Vector3> path;

    private readonly Vector3 start;

    private readonly VNavmesh vnav;

    public FollowPathChain(VNavmesh vnav, Vector3 start, List<Vector3> path)
    {
        this.vnav = vnav;
        this.start = start;
        this.path = path;
    }

    protected override Chain Create(Chain chain)
    {
        return chain
            .PathfindAndMoveTo(vnav, start)
            .WaitUntilNear(vnav, start)
            .Then(_ => vnav.FollowPath(path, false));
    }
}
