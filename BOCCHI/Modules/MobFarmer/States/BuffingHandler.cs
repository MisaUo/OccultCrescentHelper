using BOCCHI.ActionHelpers;
using BOCCHI.Modules.MobFarmer.Chains;
using Ocelot.States;

namespace BOCCHI.Modules.MobFarmer.States;

[State<FarmerPhase>(FarmerPhase.Buffing)]
public class BuffingHandler : FarmerPhaseHandler
{
    private bool HasRunBuff = false;

    public override void OnEnter(MobFarmerModule module)
    {
        HasRunBuff = false;
    }

    public override FarmerPhase? Handle(MobFarmerModule module)
    {
        if (!module.Config.ApplyBattleBell)
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
}
