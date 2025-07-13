using System.Collections.Generic;
using Ocelot.Commands;
using Ocelot.Modules;

namespace BOCCHI.Commands;

[OcelotCommand]
public class ConfigCommand(Plugin plugin) : OcelotCommand
{
    public override string command
    {
        get => "/bocchicfg";
    }

    public override string description
    {
        get => @"
Opens Occult Crescent Helper config ui
 - /bocchicfg : Opens the config ui
--------------------------------
".Trim();
    }

    public override IReadOnlyList<string> aliases
    {
        get => ["/bocchic", "/ochcfg", "/ochc", "/occultcrescenthelperconfig"];
    }


    public override void Command(string command, string arguments)
    {
        plugin.Windows.ToggleConfigUI();
    }
}
