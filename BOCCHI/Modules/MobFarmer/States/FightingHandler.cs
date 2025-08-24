using BOCCHI.Chains;
using BOCCHI.Enums;
using BOCCHI.Modules.Automator;
using BOCCHI.Modules.Buff;
using BOCCHI.Modules.MobFarmer.Chains;
using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;
using Ocelot.States;
using System.Linq;
using System.Numerics;


namespace BOCCHI.Modules.MobFarmer.States;

[State<FarmerPhase>(FarmerPhase.Fighting)]
public class FightingHandler(MobFarmerModule module) : FarmerPhaseHandler(module)
{
    public override void Enter()
    {
        base.Enter();
        var auto = Module.GetModule<AutomatorModule>();
        auto.Config.AiProvider.On();

        if (Svc.PluginInterface.InstalledPlugins.Any(p => p.InternalName == "AEAssistV3" && p.IsLoaded))
        {
            Chat.ExecuteCommand("/occs on");
            Chat.ExecuteCommand("/aeTargetSelector off");
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (Svc.PluginInterface.InstalledPlugins.Any(p => p.InternalName == "AEAssistV3" && p.IsLoaded))
        {
            Chat.ExecuteCommand("/aepull off");
        }
    }

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
        var shouldReturnHome = Module.Config.ReturnToStartInWaitingPhase && Player.DistanceTo(startingPoint) >= Module.Config.MinEuclideanDistanceToReturnHome;

        if (shouldRefreshBuffs && !anyInCombat && !Plugin.Chain.IsRunning && EzThrottler.Throttle("Fighting.BuffChain", 1000))
        {
            var vnav = Module.GetIPCSubscriber<VNavmesh>();
            var lifestream = Module.GetIPCSubscriber<Lifestream>();
            var activityShard = AethernetData.AllByDistance(startingPoint).First();

            Plugin.Chain.Submit(new ReturnTeleportWalkChain(vnav, lifestream, startingPoint, activityShard));
            return Player.DistanceTo(startingPoint) <= 2f ? FarmerPhase.Waiting : null;
        }

        if (shouldReturnHome && !anyInCombat && !Plugin.Chain.IsRunning && EzThrottler.Throttle("Fighting.ReturnHome", 500))
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

    private bool ShouldMountTo(Vector3 destination)
    {
        if (!Module.PluginConfig.TeleporterConfig.ShouldMount)
        {
            return false;
        }

        return Vector3.Distance(Player.Position, destination) > 15f;
    }
}
