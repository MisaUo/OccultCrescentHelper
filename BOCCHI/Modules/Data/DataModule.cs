using System.Linq;
using BOCCHI.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.Throttlers;
using Ocelot.Modules;

namespace BOCCHI.Modules.Data;

[OcelotModule(int.MaxValue)]
public class DataModule : Module<Plugin, Config>
{
    public override DataConfig config
    {
        get => _config.DataConfig;
    }

    private readonly Api api;

    public DataModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        api = new Api(this);
    }

    public override void PostInitialize()
    {
        api.Initialize();
    }

    public override void Tick(IFramework _)
    {
        if (!config.Enabled)
        {
            return;
        }

        if (!EzThrottler.Throttle("ApiScan", 2500))
        {
            return;
        }

        var enemies = TargetHelper.Enemies.Where(e => e.Name.TextValue.Length > 0);
        foreach (var enemy in enemies)
        {
            api.SendEnemyData(enemy);
        }

        var traps = Svc.Objects.OfType<IEventObj>()
            .Where(o => o.DataId is (uint)OccultObjectType.Trap or (uint)OccultObjectType.BigTrap or (uint)OccultObjectType.Carrot);
        foreach (var trap in traps)
        {
            api.SendTrapData(trap);
        }
    }

    public override void Dispose()
    {
        api.Dispose();
    }
}
