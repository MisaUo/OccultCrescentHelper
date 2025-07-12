using Ocelot.Modules;

namespace BOCCHI.Modules.ForkedTower;

[OcelotModule]
public class ForkedTowerModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override ForkedTowerConfig config
    {
        get => _config.ForkedTowerConfig;
    }

    private readonly Panel panel = new();


    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }
}
