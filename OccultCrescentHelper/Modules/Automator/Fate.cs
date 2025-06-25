using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using OccultCrescentHelper.Data;
using OccultCrescentHelper.Modules.Fates;
using OccultCrescentHelper.Modules.StateManager;
using Ocelot.IPC;

namespace OccultCrescentHelper.Modules.Automator;

public class Fate : Activity
{
    private IFate fate;

    public Fate(EventData data, Lifestream lifestream, VNavmesh vnav, AutomatorModule module, IFate fate)
        : base(data, lifestream, vnav, module)
    {
        this.fate = fate;
    }

    protected override unsafe TaskManagerTask GetPathfindingWatcher(StateManagerModule states, VNavmesh vnav)
    {
        var lastTargetPos = Vector3.Zero;

        return new TaskManagerTask(() => {
            if (EzThrottler.Throttle("FatePathfindingWatcher.EnemyScan", 100))
            {
                if (Svc.Targets.Target == null)
                {
                    var enemy = GetCentralMostEnemy();
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
                        // Dismount
                        if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.Mounted])
                        {
                            ActionManager.Instance()->UseAction(ActionType.Mount, module.plugin.config.MountConfig.Mount);
                        }

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

    public override Vector3 GetPosition()
    {
        return data.start ?? fate.Position;
    }
}
