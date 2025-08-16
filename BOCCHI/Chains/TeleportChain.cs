using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.Teleporter;
using Dalamud.Game.ClientState.Conditions;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;
using System.Linq;

namespace BOCCHI.Chains;

public class TeleportChain(Aethernet aethernet, Lifestream lifestream, TeleporterModule module) : ChainFactory
{
    protected override Chain Create(Chain chain)
    {
        var vnav = module.GetIPCSubscriber<VNavmesh>();
        var nearby = ZoneData.GetNearbyAethernetShards(20);
        if (nearby.Count <= 0)
        {
            return chain;
        }

        chain.Then(_ => lifestream.Abort());
        chain.BreakIf(() => nearby.Count <= 0);

        var nearest = nearby.First();
        if (lifestream.GetActiveCustomAetheryte() == 0)
        {
            chain.Then(new PathfindAndMoveToChain(vnav, nearest.Position));
            chain.Then(_ => lifestream.GetActiveCustomAetheryte() != 0);
        }

        chain.Then(_ => vnav.Stop());
        chain.Then(_ => lifestream.AethernetTeleportByPlaceNameId((uint)aethernet));
        chain.WaitToCycleCondition(ConditionFlag.BetweenAreas);
        // Mount if we should mount and not pathfind, otherwise let the pathfinder handle it
        chain.ConditionalThen(_ => module.Config is { ShouldMount: true, PathToDestination: false }, ChainHelper.MountChain());

        return chain;
    }
}
