using System.Collections.Generic;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using Ocelot;
using Ocelot.IPC;
using Ocelot.Modules;

namespace BOCCHI.Modules.Automator;

[OcelotModule(int.MaxValue - 1)]
public class AutomatorModule : Module<Plugin, Config>
{
    public override AutomatorConfig Config
    {
        get => PluginConfig.AutomatorConfig;
    }

    public override bool IsEnabled
    {
        get => Config.IsPropertyEnabled(nameof(Config.Enabled));
    }

    public readonly Automator automator = new();

    public readonly Panel panel = new();

    private readonly List<uint> occultCrescentTerritoryIds = [1252];

    public AutomatorModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        config.AutomatorConfig.Enabled = false;
        config.Save();
    }


    public override void PostUpdate(IFramework framework)
    {
        automator.PostUpdate(this, framework);
    }


    public override bool RenderMainUi()
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
        Config.Enabled = false;
        PluginConfig.Save();
    }

    public static void ToggleIllegalMode(OcelotPlugin plugin)
    {
        var module = plugin.Modules.GetModule<AutomatorModule>();
        if (!module.Config.Enabled)
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
        var wasDisabled = !Config.Enabled;
        Config.Enabled = true;

        if (wasDisabled)
        {
            Svc.Chat.Print(T("messages.on"));
        }
    }

    public void DisableIllegalMode()
    {
        var wasEnabled = Config.Enabled;
        Config.Enabled = false;
        automator.Refresh();
        Plugin.IPC.GetProvider<VNavmesh>().Stop();
        Plugin.Chain.Abort();

        if (wasEnabled)
        {
            Svc.Chat.Print(T("messages.off"));
        }
    }
}
