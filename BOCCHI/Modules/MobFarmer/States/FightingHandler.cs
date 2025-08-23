using System.Linq;
using System.Numerics;
using BOCCHI.Chains;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.Buff;
using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;
using Ocelot.States;


namespace BOCCHI.Modules.MobFarmer.States;

[State<FarmerPhase>(FarmerPhase.Fighting)]
public class FightingHandler(MobFarmerModule module) : FarmerPhaseHandler(module)
{
    public override FarmerPhase? Handle()
    {
        var anyInCombat = Module.Scanner.InCombat.Any();
        if (anyInCombat && EzThrottler.Throttle("Targetter"))
        {
            Svc.Targets.Target = Module.Scanner.InCombat.Centroid();
        }

        var buffs = Module.GetModule<BuffModule>();
        var shouldRefreshBuffs = buffs?.IsEnabled == true && buffs.ShouldRefreshBuffs();

        var startingPoint = Module.Farmer.StartingPoint;
        var shouldReturnHome =
            Module.Config.ReturnToStartInWaitingPhase &&
            Player.DistanceTo(startingPoint) >= Module.Config.MinEuclideanDistanceToReturnHome;
        
        if (shouldRefreshBuffs && !anyInCombat && !Plugin.Chain.IsRunning && EzThrottler.Throttle("Fighting.BuffChain", 1000))
        {
            var vnav = Module.GetIPCSubscriber<VNavmesh>();
            var lifestream = Module.GetIPCSubscriber<Lifestream>();
            var activityShard = AethernetData.AllByDistance(startingPoint).First();
            
            Plugin.Chain.Submit(() => BuildReturnTeleportWalkChain(vnav, lifestream, startingPoint, activityShard));
            return Player.DistanceTo(startingPoint) <= 2f ? FarmerPhase.Waiting : null;
        }
        
        if (shouldReturnHome && !anyInCombat && EzThrottler.Throttle("Fighting.ReturnHome", 500))
        {
            var vnav = Module.GetIPCSubscriber<VNavmesh>();
            if (!vnav.IsRunning())
            {
                Plugin.Chain.Submit(() =>
                        Chain.Create("ReturnHome")
                            .ConditionalThen(_ => ShouldMountTo(startingPoint), ChainHelper.MountChain())
                            .Then(new PathfindAndMoveToChain(vnav, startingPoint))
                            .WaitUntilNear(vnav, startingPoint, 2f)
                            .Then(_ => vnav.Stop())
                );
            }

            return Player.DistanceTo(startingPoint) <= 2f ? FarmerPhase.Waiting : null;
        }

        if (!anyInCombat && !Svc.Condition[ConditionFlag.InCombat])
        {
            Module.Farmer.RotationPlugin.PhantomJobOff();
            return FarmerPhase.Waiting;
        }

        return null;
    }
    
    private Chain BuildReturnTeleportWalkChain(
        VNavmesh vnav,
        Lifestream lifestream,
        Vector3 destination,
        AethernetData activityShard)
    {
        return Chain.Create("Manual:ReturnTeleportWalk")
            .Then(ChainHelper.ReturnChain(new ReturnChainConfig { ApproachAetheryte = true }))
            .Then(ChainHelper.TeleportChain(activityShard.Aethernet))
            .Debug("Waiting for lifestream to not be 'busy'")
            .Then(new TaskManagerTask(() => !lifestream.IsBusy(), new TaskManagerConfiguration { TimeLimitMS = 30000 }))
            .Then(new PathfindAndMoveToChain(vnav, destination))
            .ConditionalThen(_ => Vector3.Distance(Player.Position, destination) > 20f, ChainHelper.MountChain())
            .WaitUntilNear(vnav, destination, 2f)
            .Then(_ => vnav.Stop());
    }
    
    private bool ShouldMountTo(Vector3 destination)
    {
        if (!Module.PluginConfig.TeleporterConfig.ShouldMount)
        {
            return false;
        }

        return Vector3.Distance(Player.Position, destination) > 15f;
    }
}
