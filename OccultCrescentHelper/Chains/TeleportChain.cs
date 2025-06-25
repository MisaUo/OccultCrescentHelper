using BOCCHI.Enums;
using BOCCHI.Modules.Teleporter;
using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation.NeoTaskManager;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;

namespace BOCCHI.Chains;

public class TeleportChain : ChainFactory
{
    private Aethernet aethernet;

    private Lifestream lifestream;

    private TeleporterModule module;

    public TeleportChain(Aethernet aethernet, Lifestream lifestream, TeleporterModule module)
    {
        this.lifestream = lifestream;
        this.aethernet = aethernet;
        this.module = module;
    }

    protected override Chain Create(Chain chain)
    {
        return chain
            .Then(_ => lifestream.Abort())
            .Then(new TaskManagerTask(() => ZoneHelper.GetNearbyAethernetShards(AethernetData.DISTANCE).Count > 0,
                new TaskManagerConfiguration { TimeLimitMS = 15000 }))
            .Then(_ => module.GetIPCProvider<VNavmesh>()?.Stop())
            .Then(_ => lifestream.AethernetTeleportByPlaceNameId((uint)aethernet))
            .WaitToCycleCondition(ConditionFlag.BetweenAreas)
            .ConditionalThen(_ => module.config.ShouldMount, ChainHelper.MountChain());
    }
}
