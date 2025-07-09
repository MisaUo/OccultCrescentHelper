using System.Collections.Generic;
using System.Linq;
using BOCCHI.Data;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace BOCCHI.Modules.MobFarmer;

public class Scanner(MobFarmerModule module)
{
    public IEnumerable<IBattleNpc> Mobs { get; private set; } = [];

    public unsafe void Tick(IFramework _)
    {
        Mobs = TargetHelper.Enemies.Where(o =>
        {
            var chara = (BattleChara*)o.Address;

            if (o.NameId == (uint)module.config.Mob)
            {
                return chara->ForayInfo.Level <= module.config.MaxMobLevel;
            }

            if (!module.config.ConsiderSpecialMobs)
            {
                return false;
            }

            if (MobData.MobsWithSpawnCondition.Contains((Mob)o.NameId))
            {
                return chara->ForayInfo.Level <= module.config.MaxMobLevel;
            }

            return false;
        });
    }
}
