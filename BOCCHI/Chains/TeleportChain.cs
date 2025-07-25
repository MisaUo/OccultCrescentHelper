using System.Linq;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.Teleporter;
using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;

namespace BOCCHI.Chains;

public class TeleportChain(Aethernet aethernet, Lifestream lifestream, TeleporterModule module) : ChainFactory
{
    protected override Chain Create(Chain chain)
    {
        var vnav = module.GetIPCProvider<VNavmesh>();
        var nearby = ZoneData.GetNearbyAethernetShards(AethernetData.DISTANCE);
        if (nearby.Count <= 0)
        {
            return chain;
        }

        chain.Then(_ => lifestream.Abort());
        chain.BreakIf(() => nearby.Count <= 0);

        var nearest = nearby.First();
        if (Player.DistanceTo(nearest.Position) >= AethernetData.DISTANCE)
        {
            chain.Then(new PathfindAndMoveToChain(vnav, nearest.Position));
            chain.Then(_ => lifestream.GetActiveCustomAetheryte() != 0 && Player.DistanceTo(nearest.Position) < AethernetData.DISTANCE);
        }

        chain.Then(_ => vnav.Stop());
        chain.Then(_ => lifestream.AethernetTeleportByPlaceNameId((uint)aethernet));
        chain.WaitToCycleCondition(ConditionFlag.BetweenAreas);
        // Mount if we should mount and not pathfind, otherwise let the pathfinder handle it
        chain.ConditionalThen(_ => module.Config is { ShouldMount: true, PathToDestination: false }, ChainHelper.MountChain());

        return chain;
    }
}
