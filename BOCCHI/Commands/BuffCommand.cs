using System.Collections.Generic;
using BOCCHI.Modules.Buff;
using ECommons.DalamudServices;
using Ocelot.Commands;
using Ocelot.Modules;

namespace BOCCHI.Commands;

[OcelotCommand]
public class BuffCommand : OcelotCommand
{
    public override string command
    {
        get => "/bocchibuff";
    }

    public override string description
    {
        get => "";
    }


    private readonly Plugin plugin;

    public BuffCommand(Plugin plugin)
    {
        this.plugin = plugin;
    }


    public override void Command(string command, string arguments)
    {
        var buffs = plugin.modules.GetModule<BuffModule>();
        if (buffs == null)
        {
            Svc.Log.Error("BuffModule not found.");
            return;
        }

        buffs.buffs.QueueBuffs();
    }
}
