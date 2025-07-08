using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Schema;
using BOCCHI.Data;
using BOCCHI.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;
using Ocelot.Modules;

namespace BOCCHI.Modules.MobFarmer;

// [OcelotModule(int.MaxValue - 2)]
public class MobFarmerModule : Module<Plugin, Config>
{
    public override MobFarmerConfig config
    {
        get => _config.MobFarmerConfig;
    }

    public enum Phase
    {
        Waiting,

        Buff,

        Gather,

        Fight,
    }

    public bool Running { get; private set; } = false;

    public Phase phase { get; private set; } = Phase.Waiting;

    private Panel panel = new();

    private Scanner scanner = new();

    public IEnumerable<IGameObject> InCombat
    {
        get => scanner.Foppers.Where(o => o.TargetObject == Player.Object);
    }

    public IEnumerable<IGameObject> NotInCombat
    {
        get => scanner.Foppers.Where(o => o.TargetObject != Player.Object);
    }

    public MobFarmerModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
    }

    public override unsafe void Tick(IFramework framework)
    {
        scanner.Tick(framework);
        if (!Running)
        {
            return;
        }

        if (scanner.Foppers.Count < 10 && phase != Phase.Fight)
        {
            phase = Phase.Waiting;
            return;
        }

        if (phase == Phase.Waiting)
        {
            phase = Phase.Buff;
        }

        var vnav = GetIPCProvider<VNavmesh>();

        if (phase == Phase.Fight)
        {
            if (!InCombat.Any())
            {
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 4);
                vnav.Stop();

                phase = Phase.Buff;
            }

            if (Svc.Targets.Target == null)
            {
                Svc.Targets.Target = InCombat.First();
                return;
            }
        }

        if (phase == Phase.Buff)
        {
            if (!config.ApplyBattleBell)
            {
                phase = Phase.Gather;
                return;
            }

            if (Plugin.Chain.IsRunning)
            {
                return;
            }

            if (Svc.ClientState.LocalPlayer!.StatusList.Has(PlayerStatus.BattleBell))
            {
                Plugin.Chain.Submit(() =>
                    Chain.Create()
                        .Then(_ => PublicContentOccultCrescent.ChangeSupportJob((byte)Job.Cannoneer.id))
                        .WaitUntilStatus((uint)Job.Cannoneer.status)
                        .Then(_ => phase = Phase.Gather)
                );
                return;
            }


            Plugin.Chain.Submit(() =>
                Chain.Create()
                    .Then(_ => PublicContentOccultCrescent.ChangeSupportJob((byte)Job.Geomancer.id))
                    .WaitUntilStatus((uint)Job.Geomancer.status)
                    .WaitGcd()
                    .UseAction(ActionType.GeneralAction, 31)
            );
        }

        if (phase == Phase.Gather)
        {
            if (InCombat.Count() >= 15 || !NotInCombat.Any())
            {
                vnav.Stop();
                phase = Phase.Fight;
                return;
            }

            if (Svc.Targets.Target?.TargetObject == Player.Object)
            {
                Svc.Targets.Target = null;
            }

            Svc.Targets.Target = NotInCombat.First();

            if (Svc.Targets.Target != null)
            {
                if (Svc.Targets.Target.TargetObject == Player.Object || EzThrottler.Throttle("Remove", 2000))
                {
                    vnav.PathfindAndMoveTo(Svc.Targets.Target.Position, false);
                }
            }
        }
    }

    public override void Draw()
    {
        foreach (var fopper in NotInCombat)
        {
            Helpers.DrawLine(Player.Position, fopper.Position, 3f, new Vector4(0.9f, 0.1f, 0.1f, 1f));
        }

        foreach (var fopper in InCombat)
        {
            Helpers.DrawLine(Player.Position, fopper.Position, 3f, new Vector4(0.1f, 0.9f, 0.1f, 1f));
        }
    }

    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }

    public void Toggle()
    {
        Running = !Running;
        phase = Phase.Buff;
    }
}
