using System.Linq;
using Dalamud.Plugin.Services;
using ECommons.Throttlers;
using Ocelot.Modules;

namespace BOCCHI.Modules.Data;

[OcelotModule(int.MaxValue)]
public class DataModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override DataConfig config
    {
        get => _config.DataConfig;
    }

    public override bool tick
    {
        get => config.Enabled;
    }

    private readonly Api Api = new();

    public override void Tick(IFramework _)
    {
        if (!EzThrottler.Throttle("ApiScan", 2500))
        {
            return;
        }

        var enemies = TargetHelper.Enemies.Where(e => e.Name.TextValue.Length > 0);
        foreach (var enemy in enemies)
        {
            Api.SendEnemyData(enemy);
        }
    }
}
