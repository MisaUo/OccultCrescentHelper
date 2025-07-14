using Ocelot.Modules;
using Ocelot.Windows;

namespace BOCCHI.Modules.MobFarmer;

[OcelotModule(int.MaxValue - 2)]
public class MobFarmerModule : Module
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

    public override void Update(UpdateContext context)
    {
        scanner.Tick(context.Framework);
        farmer.Tick();
    }

    public override void Render(RenderContext context)
    {
        farmer.Draw();
    }

    public override bool RenderMainUi(RenderContext context)
    {
        panel.Draw(this);
        return true;
    }
}
