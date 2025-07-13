using Dalamud.Plugin.Services;
using Ocelot.Modules;

namespace BOCCHI.Modules.MobFarmer;

[OcelotModule(int.MaxValue - 2)]
public class MobFarmerModule : Module<Plugin, Config>
{
    public override MobFarmerConfig Config
    {
        get => PluginConfig.MobFarmerConfig;
    }

    public override bool IsEnabled
    {
        get => Config.Enabled;
    }

    private readonly Panel panel = new();

    public readonly Scanner scanner;

    public readonly Farmer farmer;

    public MobFarmerModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        scanner = new Scanner(this);
        farmer = new Farmer(this);
    }

    public override void Update(IFramework framework)
    {
        scanner.Tick(framework);
        farmer.Tick();
    }

    public override void Render()
    {
        farmer.Draw();
    }

    public override bool RenderMainUi()
    {
        panel.Draw(this);
        return true;
    }
}
