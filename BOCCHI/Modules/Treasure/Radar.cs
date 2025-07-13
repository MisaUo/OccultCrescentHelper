using System.Linq;
using BOCCHI.Data;
using BOCCHI.Enums;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace BOCCHI.Modules.Treasure;

public class Radar
{
    public void Draw(TreasureModule module)
    {
        if (!ZoneData.IsInOccultCrescent() || Svc.Condition[ConditionFlag.InCombat])
        {
            return;
        }

        var config = module.Config;
        if (!config.ShouldDrawLineToBronzeChests && !config.ShouldDrawLineToSilverChests)
        {
            return;
        }

        var pos = Player.Position;
        foreach (var treasure in module.Treasures.Where(treasure => treasure.IsValid()))
        {
            switch (treasure.GetTreasureType())
            {
                case TreasureType.Bronze:
                    if (config.ShouldDrawLineToBronzeChests)
                    {
                        Helpers.DrawLine(pos, treasure.GetPosition(), 3f, treasure.GetColor());
                    }

                    break;

                case TreasureType.Silver:
                    if (config.ShouldDrawLineToSilverChests)
                    {
                        Helpers.DrawLine(pos, treasure.GetPosition(), 3f, treasure.GetColor());
                    }

                    break;
            }
        }
    }
}
