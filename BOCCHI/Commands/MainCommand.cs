using System.Collections.Generic;
using BOCCHI.Modules.Buff;
using BOCCHI.Modules.Debug;
using ECommons.DalamudServices;
using Ocelot.Commands;
using Ocelot.Modules;

namespace BOCCHI.Commands;

[OcelotCommand]
public class MainCommand(Plugin plugin) : OcelotCommand
{
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
        get => ["config", "cfg", "debug", "buff", "tp"];
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

        if (arguments == "buff")
        {
            var buffs = plugin.modules.GetModule<BuffModule>();
            if (buffs != null)
            {
                buffs.buffs.QueueBuffs();
                return;
            }
        }

        if (arguments == "tp")
        {
            Svc.Chat.Print("Press the button nerd.");
            return;
        }

        plugin.windows?.ToggleMainUI();
    }
}
