﻿using System.Collections.Generic;
using System.Linq;
using BOCCHI.Data;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace BOCCHI.Modules.MobFarmer;

public class Scanner(MobFarmerModule module)
{
    public IEnumerable<IBattleNpc> Mobs { get; private set; } = [];

    public unsafe void Tick(IFramework _)
    {
        Mobs = TargetHelper.Enemies.Where(o =>
        {
            if (Player.DistanceTo(o) > module.Config.MaxEuclideanDistance)
            {
                return false;
            }

            var chara = (BattleChara*)o.Address;

            if (o.NameId == (uint)module.Config.Mob)
            {
                return chara->ForayInfo.Level <= module.Config.MaxMobLevel;
            }

            if (!module.Config.ConsiderSpecialMobs)
            {
                return false;
            }

            if (MobData.MobsWithSpawnCondition.Contains((Mob)o.NameId))
            {
                return chara->ForayInfo.Level <= module.Config.MaxMobLevel;
            }

            return false;
        });
    }
}
