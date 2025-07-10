using System;
using System.Linq;
using System.Numerics;
using BOCCHI.ActionHelpers;
using BOCCHI.Data;
using BOCCHI.Modules.Fates;
using BOCCHI.Modules.StateManager;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ocelot.IPC;

namespace BOCCHI.Modules.Automator;

public class Fate(EventData data, Lifestream lifestream, VNavmesh vnav, AutomatorModule module, IFate fate)
    : Activity(data, lifestream, vnav, module)
{
    protected override TaskManagerTask GetPathfindingWatcher(StateManagerModule states)
    {
        var lastTargetPos = Vector3.Zero;

        return new TaskManagerTask(() =>
        {
            if (EzThrottler.Throttle("FatePathfindingWatcher.EnemyScan", 100))
            {
                if (Svc.Targets.Target == null)
                {
                    var enemy = GetEnemies().Centroid();
                    if (enemy != null)
                    {
                        Svc.Targets.Target = enemy;
                    }
                }
            }

            var target = Svc.Targets.Target;
            if (target != null)
            {
                if (Vector3.Distance(target.Position, lastTargetPos) > 5f)
                {
                    vnav.PathfindAndMoveTo(target.Position, false);
                    lastTargetPos = target.Position;
                }

                if (states.GetState() == State.InFate)
                {
                    if (Vector3.Distance(Player.Position, target.Position) <= module.config.EngagementRange)
                    {
                        Actions.TryUnmount();

                        vnav.Stop();

                        return true;
                    }
                }
            }

            if (!vnav.IsRunning())
            {
                throw new VnavmeshStoppedException();
            }

            return false;
        }, new TaskManagerConfiguration { TimeLimitMS = 180000, ShowError = false });
    }

    protected override float GetRadius()
    {
        return module.GetModule<FatesModule>().fates[data.id].Radius;
    }

    public override bool IsValid()
    {
        return Svc.Fates.Contains(fate);
    }

    protected override Vector3 GetPosition()
    {
        return data.start ?? fate.Position;
    }

    public override string GetName()
    {
        try
        {
            return fate.Name.ToString();
        }
        catch (AccessViolationException)
        {
            return data.Name;
        }
    }

    protected override unsafe bool IsActivityTarget(IBattleNpc obj)
    {
        try
        {
            var battleChara = (BattleChara*)obj.Address;

            return battleChara->FateId == data.id;
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex.Message);
            return false;
        }
    }

    protected override ActivityState GetPostPathfindingState()
    {
        return ActivityState.Participating;
    }
}
