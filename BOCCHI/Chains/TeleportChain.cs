using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.Teleporter;
using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation.NeoTaskManager;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;

namespace BOCCHI.Chains;

public class TeleportChain(Aethernet aethernet, Lifestream lifestream, TeleporterModule module) : ChainFactory
{
    protected override Chain Create(Chain chain)
    {
        return chain
            .Then(_ => lifestream.Abort())
            .Then(new TaskManagerTask(() => ZoneData.GetNearbyAethernetShards(AethernetData.DISTANCE).Count > 0))
            .Then(_ => module.GetIPCProvider<VNavmesh>()?.Stop())
            .Then(_ => lifestream.AethernetTeleportByPlaceNameId((uint)aethernet))
            .WaitToCycleCondition(ConditionFlag.BetweenAreas)
            // Mount if we should mount and not pathfind, otherwise let the pathfinder handle it
            .ConditionalThen(_ => module.Config is { ShouldMount: true, PathToDestination: false }, ChainHelper.MountChain());
    }
}
