using System.Collections.Generic;
using Ocelot.Modules;
using Ocelot.Commands;
using OccultCrescentHelper.Modules.Debug;

namespace OccultCrescentHelper.Commands;

[OcelotCommand]
public class MainCommand : OcelotCommand
{
    public override string command => "/bocchi";

    public override string description => @"
Opens Occult Crescent Helper main ui
 - /bocchi : Opens the main ui
 - /bocchi config : opens the config ui
 - /bocchi cfg : opens the config ui
--------------------------------
".Trim();

    public override IReadOnlyList<string> aliases => ["/och", "/occultcrescenthelper"];

    public override IReadOnlyList<string> validArguments => ["config", "cfg", "debug"];


    private readonly Plugin plugin;

    public MainCommand(Plugin plugin)
    {
        this.plugin = plugin;
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
