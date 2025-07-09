using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Pathfinding;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using Ocelot.Chain;

namespace BOCCHI.Modules.Carrots;

public class CarrotHunt(CarrotsModule module) : Hunter(module)
{
    protected override IEnumerable<IGameObject> GetValidObjects()
    {
        return Svc.Objects
            .Where(o => o is
            {
                ObjectKind: ObjectKind.EventObj,
                DataId: (uint)OccultObjectType.Carrot,
                IsDead: false,
            } && o.IsValid());
    }

    protected override Vector3 GetDestinationForCurrentStep()
    {
        return CarrotData.Data.First(c => c.Id == CurrentStep.NodeId).Position;
    }

    protected override IPathfinder CreatePathfinder()
    {
        return new Pathfinder(module._config.PathfinderConfig.ReturnCost, module._config.PathfinderConfig.TeleportCost);
    }

    protected override Func<Chain> GetInteractionChain(IGameObject obj)
    {
        return () => Chain.Create();
    }

    protected override List<uint> GetValidNodes(int max)
    {
        return CarrotData.Data.Where(node => node.Level <= max).Select(node => node.Id).ToList();
    }

    protected override void Teardown()
    {
        // if (!module.config.RepeatCarrotHunt)
        // {
        //     stopwatch.Stop();
        //     running = false;
        // }

        stepIndex = 0;
        Steps.Clear();
        vnav.Stop();
        Plugin.Chain.Abort();
        StepProcessor.Abort();
        pathfinder = null;
    }
}
