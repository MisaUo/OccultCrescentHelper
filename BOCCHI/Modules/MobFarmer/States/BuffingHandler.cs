using BOCCHI.ActionHelpers;
using BOCCHI.Modules.MobFarmer.Chains;
using Ocelot.IPC;
using Ocelot.States;

namespace BOCCHI.Modules.MobFarmer.States;

[State<FarmerPhase>(FarmerPhase.Buffing)]
public class BuffingHandler(MobFarmerModule module) : FarmerPhaseHandler(module)
{
    private bool HasRunBuff = false;

    public override void Enter()
    {
        base.Enter();
        HasRunBuff = false;
    }

    public override FarmerPhase? Handle()
    {
        var vnav = Module.GetIPCSubscriber<VNavmesh>();
        if (vnav.IsRunning())
        {
            return null;
        }
                
        if (Plugin.Chain.IsRunning)
        {
            return null;
        }
        
        if (!Module.Config.ApplyBattleBell)
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
            if (Actions.Sprint.GetRecastTime() == 0)
            {
                Plugin.Chain.Submit(Actions.Sprint.GetCastChain());
            }
            return FarmerPhase.Gathering;
        }

        Plugin.Chain.Submit(new BattleBellChain(Module));
        HasRunBuff = true;

        return null;
    }
}
