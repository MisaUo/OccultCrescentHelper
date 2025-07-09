using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace BOCCHI.Modules.MobFarmer;

public class Scanner(MobFarmerModule module)
{
    public IEnumerable<IBattleNpc> Mobs { get; private set; } = [];

    public void Tick(IFramework _)
    {
        Mobs = TargetHelper.Enemies.Where(o => o.NameId == (uint)module.config.Mob);
    }
}
