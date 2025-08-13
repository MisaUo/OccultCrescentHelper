using Ocelot.Commands;
using Ocelot.Modules;
using System.Collections.Generic;

namespace BOCCHI.Commands;

[OcelotCommand]
public class ConfigCommand(Plugin plugin) : OcelotCommand
{
    protected override string Command
    {
        get => "/bocchicfg";
    }

    protected override string Description
    {
        get => @"
打开 Occult Crescent Helper 设置界面
 - /bocchicfg : 打开设置界面
--------------------------------
".Trim();
    }

    protected override IReadOnlyList<string> Aliases
    {
        get => ["/bocchic", "/ochcfg", "/ochc", "/occultcrescenthelperconfig"];
    }


    public override void Execute(string command, string arguments)
    {
        plugin.Windows.ToggleConfigUI();
    }
}
