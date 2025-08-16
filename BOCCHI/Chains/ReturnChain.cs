using BOCCHI.ActionHelpers;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.Buff;
using BOCCHI.Modules.Buff.Chains;
using BOCCHI.Modules.Teleporter;
using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;
using System;
using System.Linq;
using System.Numerics;

namespace BOCCHI.Chains;

public class ReturnChain(TeleporterModule module, ReturnChainConfig config) : RetryChainFactory
{
    private bool complete = false;

    protected override Chain Create(Chain chain)
    {
        chain.BreakIf(() => Player.IsDead);

        var shouldReturn = GetCostToReturn() < GetCostToWalk();

        if (shouldReturn)
        {
            chain = Actions.Return.CastOnChain(chain);
            chain.WaitToCast().WaitToCycleCondition(ConditionFlag.BetweenAreas);
        }

        chain.Then(ChainHelper.TreasureSightChain());
        chain.Then(ApplyBuffs);

        if (config.ApproachAetheryte)
        {
            var vnav = module.GetIPCSubscriber<VNavmesh>();
            var lifestream = module.GetIPCSubscriber<Lifestream>();
            var position = GetAetherytePosition();

            chain.Then(PathfindAndMoveToChain.RandomNearby(vnav, position, AethernetData.DISTANCE, 3));
            chain.Then(_ => lifestream.GetActiveCustomAetheryte() != 0 && Player.DistanceTo(position) <= AethernetData.DISTANCE);
            chain.Then(_ => vnav.Stop());
        }


        return chain.Then(_ => complete = true);
    }

    private Chain ApplyBuffs()
    {
        var vnav = module.GetIPCSubscriber<VNavmesh>();
        var buffs = module.GetModule<BuffModule>();

        var closestKnowledgeCrystal = ZoneData.GetNearbyKnowledgeCrystal(60f).FirstOrDefault();

        var chain = Chain.Create();

        var random = new Random();
        const float minAbs = 0.5f;
        const float maxAbs = 2f;

        var positiveX = random.Next(0, 2) == 1;
        var magnitudeX = (float)(minAbs + random.NextDouble() * (maxAbs - minAbs));
        var valueX = positiveX ? magnitudeX : -magnitudeX;

        var positiveZ = random.Next(0, 2) == 1;
        var magnitudeZ = (float)(minAbs + random.NextDouble() * (maxAbs - minAbs));
        var valueZ = positiveZ ? magnitudeZ : -magnitudeZ;


        chain.BreakIf(() => !buffs.ShouldRefreshBuffs() || !vnav.IsReady() || closestKnowledgeCrystal == null);
        chain.Then(_ => Actions.TryUnmount());

        chain.PathfindAndMoveTo(vnav, closestKnowledgeCrystal!.Position + new Vector3(valueX, 0, valueZ));
        chain.WaitUntilNear(vnav, closestKnowledgeCrystal!.Position, AethernetData.DISTANCE);
        chain.Then(_ => vnav.Stop());

        chain.Then(new AllBuffsChain(buffs));

        return chain;
    }

    public override bool IsComplete()
    {
        return complete;
    }

    public override int GetMaxAttempts()
    {
        return 5;
    }

    public override TaskManagerConfiguration? Config()
    {
        return new TaskManagerConfiguration { TimeLimitMS = 60000 };
    }

    private Vector3 GetAetherytePosition()
    {
        if (ZoneData.Aetherytes.TryGetValue(Svc.ClientState.TerritoryType, out var position))
        {
            return position;
        }

        throw new Exception("Unable to determine Aetheryte position");
    }

    private float GetCostToReturn()
    {
        if (ZoneData.StartingLocations.TryGetValue(Svc.ClientState.TerritoryType, out var start))
        {
            return Vector3.Distance(start, GetAetherytePosition()) + 75f;
        }


        throw new Exception("Unable to determine Starting position");
    }

    private float GetCostToWalk()
    {
        return Player.DistanceTo(GetAetherytePosition());
    }
}
