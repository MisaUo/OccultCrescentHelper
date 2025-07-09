using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.MobFarmer;

[OcelotModule(int.MaxValue - 2)]
public class MobFarmerModule : Module<Plugin, Config>
{
    public override MobFarmerConfig config
    {
        get => _config.MobFarmerConfig;
    }


    private Panel panel = new();

    public readonly Scanner scanner;

    public readonly Farmer farmer;


    public MobFarmerModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        scanner = new Scanner(this);
        farmer = new Farmer(this);
    }

    public override void Tick(IFramework framework)
    {
        scanner.Tick(framework);
        farmer.Tick();
    }

    public override void Draw()
    {
        farmer.Draw();
    }

    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }
}
