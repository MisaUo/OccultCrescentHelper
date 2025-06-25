using System.Collections.Generic;
using BOCCHI.Modules.Debug;
using Ocelot.Commands;
using Ocelot.Modules;

namespace BOCCHI.Commands;

[OcelotCommand]
public class MainCommand : OcelotCommand
{
    private readonly Plugin plugin;

    public MainCommand(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public override string command
    {
        get => "/bocchi";
    }

    public override string description
    {
        get => @"
Opens Occult Crescent Helper main ui
 - /bocchi : Opens the main ui
 - /bocchi config : opens the config ui
 - /bocchi cfg : opens the config ui
--------------------------------
".Trim();
    }

    public override IReadOnlyList<string> aliases
    {
        get => ["/och", "/occultcrescenthelper"];
    }

    public override IReadOnlyList<string> validArguments
    {
        get => ["config", "cfg", "debug"];
    }


    public override void Command(string command, string arguments)
    {
        if (arguments == "config" || arguments == "cfg")
        {
            plugin.windows?.ToggleConfigUI();
            return;
        }

#if DEBUG_BUILD
        if (arguments == "debug")
        {
            var debug = plugin.modules.GetModule<DebugModule>();
            var window = plugin.windows.GetWindow<DebugWindow>();
            if (debug != null && window != null)
            {
                window.Toggle();
                return;
            }
        }
#endif

        plugin.windows?.ToggleMainUI();
    }
}
