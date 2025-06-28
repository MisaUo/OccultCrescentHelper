using System.Collections.Generic;
using BOCCHI.Modules.Buff;
using ECommons.DalamudServices;
using Ocelot.Commands;
using Ocelot.Modules;

namespace BOCCHI.Commands;

[OcelotCommand]
public class BuffCommand(Plugin plugin) : OcelotCommand
{
    public override string command
    {
        get => "/bocchibuff";
    }

    public override string description
    {
        get => "";
    }


    public override void Command(string command, string arguments)
    {
        plugin.modules.GetModule<BuffModule>().buffs.QueueBuffs();
    }
}
