using BOCCHI.Modules.MobFarmer;
using Ocelot.Commands;
using Ocelot.Modules;
using System.Collections.Generic;

namespace BOCCHI.Commands;

[OcelotCommand]
public class OCHMobFarmerCommand(Plugin plugin) : OcelotCommand
{
    protected override string Command
    {
        get => "/bocchimobfarmer";
    }

    protected override string Description
    {
        get => @"
管理刷怪模式。
 - /bocchimobfarmer on（启用刷怪模式）
 - /bocchimobfarmer off（禁用刷怪模式）
 - /bocchimobfarmer toggle（切换刷怪模式）
--------------------------------
".Trim();
    }

    protected override IReadOnlyList<string> ValidArguments
    {
        get => ["on", "off", "toggle"];
    }

    public override void Execute(string command, string arguments)
    {

        if (!plugin.Modules.TryGetModule<MobFarmerModule>(out var mobfarmer) || mobfarmer == null)
        {
            return;
        }

        switch (arguments)
        {
            case "on":
                mobfarmer.Farmer.EnableFarmerMode();
                break;
            case "off":
                mobfarmer.Farmer.DisableFarmerMode();
                break;
            case "toggle":
                mobfarmer.Farmer.Toggle();
                break;
        }

        plugin.Config.Save();
    }
}
