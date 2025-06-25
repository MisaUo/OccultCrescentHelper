using System.Collections.Generic;
using BOCCHI.Modules.StateManager;
using Ocelot.Modules;

namespace BOCCHI.Modules.WindowManager;

[OcelotModule]
public class WindowManagerModule : Module<Plugin, Config>
{
    public override WindowManagerConfig config
    {
        get => _config.WindowManagerConfig;
    }

    public WindowManagerModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
    }


    private List<uint> occultCrescentTerritoryIds = [1252];

    private bool mainClosed = false;

    private bool configClosed = false;


    public override void PostInitialize()
    {
        if (config.OpenMainOnStartUp)
        {
            plugin.windows.OpenMainUI();
        }


        if (config.OpenConfigOnStartUp)
        {
            plugin.windows.OpenConfigUI();
        }

        GetModule<StateManagerModule>().OnEnterInCombat += EnterCombat;
        GetModule<StateManagerModule>().OnEnterInCriticalEncounter += EnterCombat;
        GetModule<StateManagerModule>().OnEnterInFate += EnterCombat;
        GetModule<StateManagerModule>().OnEnterIdle += ExitCombat;
    }

    public override void OnTerritoryChanged(ushort id)
    {
        if (occultCrescentTerritoryIds.Contains(id))
        {
            if (config.OpenMainOnEnter)
            {
                plugin.windows.OpenMainUI();
            }


            if (config.OpenConfigOnEnter)
            {
                plugin.windows.OpenConfigUI();
            }
        }
        else
        {
            if (config.CloseMainOnExit)
            {
                plugin.windows.CloseMainUI();
            }


            if (config.CloseConfigOnExit)
            {
                plugin.windows.CloseConfigUI();
            }
        }
    }

    private void EnterCombat()
    {
        if (config.HideMainInCombat && plugin.windows.IsMainUIOpen())
        {
            plugin.windows.CloseMainUI();
            mainClosed = true;
        }

        if (config.HideConfigInCombat && plugin.windows.IsConfigUIOpen())
        {
            plugin.windows.CloseConfigUI();
            configClosed = true;
        }
    }

    private void ExitCombat()
    {
        if (config.HideMainInCombat && mainClosed)
        {
            plugin.windows.OpenMainUI();
            mainClosed = false;
        }

        if (config.HideConfigInCombat && configClosed)
        {
            plugin.windows.OpenConfigUI();
            configClosed = false;
        }
    }
}
