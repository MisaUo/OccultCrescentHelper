using System.Collections.Generic;
using Ocelot.Commands;
using Ocelot.Modules;

namespace BOCCHI.Commands;

[OcelotCommand]
public class ConfigCommand : OcelotCommand
{
    private readonly Plugin plugin;

    public ConfigCommand(Plugin plugin)
    {
        this.plugin = plugin;
    }

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
        plugin.windows?.ToggleConfigUI();
    }
}
