using System;
using Dalamud.Memory;
using OccultCrescentHelper.Memory;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.InstanceIdentifier;

[OcelotModule(mainOrder: int.MinValue)]
public class InstanceIdentifierModule : Module<Plugin, Config>
{
    public override InstanceIdentifierConfig config {
        get => _config.InstanceIdentifierConfig;
    }

    public override bool enabled {
        get => config.Show;
    }

    private Panel panel = new();

    public string instance { get; private set; } = "Unknown";

    public InitZone hook = new();

    public InstanceIdentifierModule(Plugin plugin, Config config)
        : base(plugin, config) { }

    public override void PostInitialize()
    {
        hook.OnInitZone += (a1, a2, a3) => {
            try
            {
                instance = MemoryHelper.Read<ushort>(a3).ToString();
            }
            catch (Exception ex)
            {
                Error($"Something went wrong. Please contact the author.\n{ex.Message}");
            }

            return 0;
        };

        hook.Enable();
    }

    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }

    public override void Dispose()
    {
        hook.Disable();
    }
}
