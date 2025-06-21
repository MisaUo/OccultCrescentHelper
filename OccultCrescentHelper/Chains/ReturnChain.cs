using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using OccultCrescentHelper.Data;
using OccultCrescentHelper.Enums;
using OccultCrescentHelper.Modules.Buff;
using OccultCrescentHelper.Modules.Buff.Chains;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;

namespace OccultCrescentHelper.Chains;

public class ReturnChain : RetryChainFactory
{
    private bool approachAetherye = false;

    private Vector3 destination = Vector3.Zero;

    private YesAlready? yes;

    private VNavmesh? vnav;

    private BuffModule buffs;

    private bool complete = false;

    public ReturnChain(Vector3 destination, BuffModule buffs, YesAlready? yes = null, VNavmesh? vnav = null, bool approachAetherye = true)
    {
        this.destination = destination;
        this.yes = yes;
        this.vnav = vnav;
        this.approachAetherye = approachAetherye;
        this.buffs = buffs;
    }

    protected unsafe override Chain Create(Chain chain)
    {
        chain.BreakIf(() => Svc.ClientState.LocalPlayer?.IsDead == true);

        var zone = Svc.ClientState.TerritoryType;
        var costToReturn = 60f + Vector3.Distance(ZoneData.startingLocations[zone], destination);
        var costToWalk = Vector3.Distance(Player.Position, destination);

        if (costToReturn < costToWalk || vnav == null)
        {
            yes?.PausePlugin(5000);

            chain
                .UseGcdAction(ActionType.GeneralAction, 8)
                .AddonCallback("SelectYesno", true, 0)
                .WaitToCast()
                .WaitToCycleCondition(ConditionFlag.BetweenAreas);

            chain = ApplyBuffs(chain);

            if (approachAetherye && vnav != null)
            {
                chain
                    .Wait(500)
                    .Then(ChainHelper.MoveToAndWait(destination, AethernetData.DISTANCE))
                    .Then(_ => vnav.Stop());
            }
        }
        else if (vnav != null)
        {
            chain = ApplyBuffs(chain);

            chain
                .Then(ChainHelper.PathfindToAndWait(destination, AethernetData.DISTANCE))
                .Then(_ => vnav.Stop());
        }


        return chain.Then(_ => complete = true);
    }

    private unsafe Chain ApplyBuffs(Chain chain)
    {
        if (buffs.ShouldRefreshBuffs() && vnav != null)
        {
            chain.Then(() => {
                IGameObject? closestKnowledgeCrystal = ZoneHelper.GetNearbyKnowledgeCrystal(60f).FirstOrDefault();
                Vector3 position = closestKnowledgeCrystal?.Position ?? Vector3.Zero;

                return Chain.Create("Go to Crystal and Buff")
                    .BreakIf(() => !buffs.buffs.ShouldRefresh(buffs))
                    .Wait(500)
                    .Then(_ => {
                        if (Svc.Condition[ConditionFlag.Mounted])
                        {
                            ActionManager.Instance()->UseAction(ActionType.Mount, buffs.plugin.config.MountConfig.Mount);
                        }
                    })
                    .BreakIf(() => closestKnowledgeCrystal == null)
                    .Then(_ => vnav.MoveToPath([position], false))
                    .WaitUntilNear(vnav, position, AethernetData.DISTANCE)
                    .Then(_ => vnav.Stop())
                    .Then(new AllBuffsChain(buffs))
                    .Wait(2500);
            });
        }

        return chain;
    }

    public override bool IsComplete() => complete;

    public override int GetMaxAttempts() => 5;

    public override TaskManagerConfiguration? Config()
    {
        return new() { TimeLimitMS = 60000 };
    }
}
