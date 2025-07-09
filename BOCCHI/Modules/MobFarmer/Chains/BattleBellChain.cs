using BOCCHI.ActionHelpers;
using BOCCHI.Data;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;

namespace BOCCHI.Modules.MobFarmer.Chains;

public class BattleBellChain(MobFarmerModule module) : ChainFactory
{
    protected override unsafe Chain Create(Chain chain)
    {
        chain.BreakIf(() => Actions.BattleBell.GetRecastTime() >= module.config.MaximumBattleBellWaitTime);

        chain.SubChain(sub => sub
            .RunIf(() => PublicContentOccultCrescent.GetState()->CurrentSupportJob != Job.Geomancer.ByteId)
            .Then(_ => PublicContentOccultCrescent.ChangeSupportJob(Job.Geomancer.ByteId))
            .WaitUntilStatus(Job.Geomancer.UintStatus)
        );

        chain.Then(Actions.BattleBell.GetCastChain()).Wait(1000);

        chain
            .Then(_ => PublicContentOccultCrescent.ChangeSupportJob(Job.Cannoneer.ByteId))
            .WaitUntilStatus(Job.Cannoneer.UintStatus);

        return chain;
    }
}
