using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BOCCHI.ActionHelpers;
using BOCCHI.Modules.MobFarmer.Chains;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using Ocelot.Chain;
using Ocelot.IPC;
using Chain = Ocelot.Chain.Chain;

namespace BOCCHI.Modules.MobFarmer;

public class Farmer
{
    private MobFarmerModule module;

    private ChainQueue ChainQueue
    {
        get => ChainManager.Get("MobFarmer+Farmer");
    }

    public bool Running { get; private set; } = false;


    private bool HasRunBuff = false;

    private bool HasRunStack = false;

    public FarmerPhase Phase { get; private set; } = FarmerPhase.Waiting;

    public IEnumerable<IBattleNpc> Mobs
    {
        get => module.scanner.Mobs;
    }

    public IEnumerable<IBattleNpc> InCombat
    {
        get => Mobs.Where(o => o.IsTargetingPlayer());
    }

    public IEnumerable<IBattleNpc> NotInCombat
    {
        get => Mobs.Where(o => !o.HasTarget());
    }

    private Dictionary<FarmerPhase, Func<FarmerPhase?>> Handlers;

    public Farmer(MobFarmerModule module)
    {
        this.module = module;

        Handlers = new Dictionary<FarmerPhase, Func<FarmerPhase?>>
        {
            { FarmerPhase.Waiting, HandleWaitingPhase },
            { FarmerPhase.Buffing, HandleBuffingPhase },
            { FarmerPhase.Gathering, HandleGatheringPhase },
            { FarmerPhase.Stacking, HandleStackingPhase },
            { FarmerPhase.Fighting, HandleFightingPhase },
        };
    }

    public void Tick()
    {
        if (!Running || !Mobs.Any())
        {
            return;
        }

        if (!Handlers.TryGetValue(Phase, out var handler))
        {
            return;
        }

        var transition = handler();
        if (transition != null)
        {
            Phase = transition.Value;
        }
    }

    private FarmerPhase? HandleWaitingPhase()
    {
        if (Svc.Condition[ConditionFlag.InCombat])
        {
            return FarmerPhase.Fighting;
        }

        return Mobs.Count() >= module.config.MinimumMobsToStartLoop ? FarmerPhase.Buffing : null;
    }

    private FarmerPhase? HandleBuffingPhase()
    {
        if (!module.config.ApplyBattleBell)
        {
            return FarmerPhase.Gathering;
        }

        if (Plugin.Chain.IsRunning)
        {
            return null;
        }

        if (HasRunBuff)
        {
            HasRunBuff = false;
            Plugin.Chain.Submit(Actions.Sprint.GetCastChain());
            return FarmerPhase.Gathering;
        }

        Plugin.Chain.Submit(new BattleBellChain(module));
        HasRunBuff = true;

        return null;
    }

    private FarmerPhase? HandleGatheringPhase()
    {
        var vnav = module.GetIPCProvider<VNavmesh>();

        if (InCombat.Count() >= module.config.MinimumMobsToStartFight || !NotInCombat.Any())
        {
            vnav.Stop();
            ChainQueue.Abort();
            return FarmerPhase.Stacking;
        }

        if (Svc.Targets.Target?.IsTargetingPlayer() == true)
        {
            Svc.Targets.Target = null;
            ChainQueue.Abort();
        }

        Svc.Targets.Target = NotInCombat.First();

        if (!ChainQueue.IsRunning && Svc.Targets.Target != null)
        {
            var target = Svc.Targets.Target;

            if (target.IsTargetingPlayer() || EzThrottler.Throttle("Repath", 500))
            {
                ChainQueue.Submit(() =>
                    Chain.Create()
                        .Then(async void (_) =>
                        {
                            var path = await vnav.Pathfind(Player.Position, target.Position, false);
                            if (path.Count <= 1)
                            {
                                return;
                            }

                            path.RemoveAt(0);
                            vnav.MoveToPath(path, false);
                        })
                );
            }
        }

        return null;
    }

    private FarmerPhase? HandleStackingPhase()
    {
        var vnav = module.GetIPCProvider<VNavmesh>();

        if (HasRunStack && !vnav.IsRunning())
        {
            HasRunStack = false;
            Chat.ExecuteCommand("/wrath set 110058");
            return FarmerPhase.Fighting;
        }

        var furthest = InCombat.Where(o => o.Address != Svc.Targets.Target?.Address).OrderBy(Player.DistanceTo).Last();
        vnav.PathfindAndMoveTo(furthest.Position, false);
        HasRunStack = true;

        return null;
    }

    private FarmerPhase? HandleFightingPhase()
    {
        if (EzThrottler.Throttle("Targetter"))
        {
            Svc.Targets.Target = InCombat.Centroid();
        }

        return !InCombat.Any() ? FarmerPhase.Waiting : null;
    }

    public void Draw()
    {
        if (!Mobs.Any())
        {
            return;
        }

        if (!Running && !module.config.ShouldRenderDebugLinesWhileNotRunning)
        {
            return;
        }

        if (!module.config.RenderDebugLines)
        {
            return;
        }

        foreach (var mob in NotInCombat)
        {
            Helpers.DrawLine(Player.Position, mob.Position, 3f, new Vector4(0.9f, 0.1f, 0.1f, 1f));
        }

        foreach (var mob in InCombat)
        {
            Helpers.DrawLine(Player.Position, mob.Position, 3f, new Vector4(0.1f, 0.9f, 0.1f, 1f));
        }
    }

    public void Toggle()
    {
        Running = !Running;
        Phase = FarmerPhase.Waiting;
        if (Running)
        {
            Svc.Commands.ProcessCommand("/wrath unset 110058");
        }
    }
}
