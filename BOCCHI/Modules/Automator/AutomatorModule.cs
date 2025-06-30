using System.Collections.Generic;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using Ocelot;
using Ocelot.IPC;
using Ocelot.Modules;

namespace BOCCHI.Modules.Automator;

[OcelotModule]
public class AutomatorModule : Module<Plugin, Config>
{
    public override AutomatorConfig config
    {
        get => _config.AutomatorConfig;
    }

    public override bool enabled
    {
        get => config.IsPropertyEnabled(nameof(config.Enabled));
    }

    public readonly Automator automator = new();

    public readonly Panel panel = new();

    private List<uint> occultCrescentTerritoryIds = [1252];

    public AutomatorModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        config.AutomatorConfig.Enabled = false;
        config.Save();
    }


    public override void Tick(IFramework framework)
    {
        automator.Tick(this, framework);
    }


    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }

    public override void OnTerritoryChanged(ushort id)
    {
        if (occultCrescentTerritoryIds.Contains(id))
        {
            return;
        }

        automator.Refresh();
        config.Enabled = false;
        plugin.config.Save();
    }

    public static void ToggleIllegalMode(OcelotPlugin plugin)
    {
        var module = plugin.modules.GetModule<AutomatorModule>();
        if (!module.config.Enabled)
        {
            module.EnableIllegalMode();
        }
        else
        {
            module.DisableIllegalMode();
        }
    }

    public void EnableIllegalMode()
    {
        var wasDisabled = !config.Enabled;
        config.Enabled = true;

        if (wasDisabled)
        {
            Svc.Chat.Print("[BOCCHI] Illegal Mode On");
        }
    }

    public void DisableIllegalMode()
    {
        var wasEnabled = config.Enabled;
        config.Enabled = false;
        automator.Refresh();
        plugin.ipc.GetProvider<VNavmesh>()?.Stop();
        Plugin.Chain.Abort();

        if (wasEnabled)
        {
            Svc.Chat.Print("[BOCCHI] Illegal Mode Off");
        }
    }
}
