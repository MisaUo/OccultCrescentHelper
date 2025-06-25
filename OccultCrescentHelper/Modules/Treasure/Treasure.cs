using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using OccultCrescentHelper.Enums;
using XIVTreasure = Lumina.Excel.Sheets.Treasure;

namespace OccultCrescentHelper.Modules.Treasure;

public class Treasure
{
    private readonly IGameObject gameObject;

    public Treasure(IGameObject obj)
    {
        gameObject = obj;
    }

    private XIVTreasure? GetData()
    {
        return Svc.Data.GetExcelSheet<XIVTreasure>().ToList().FirstOrDefault(t => t.RowId == gameObject.DataId);
    }

    public bool IsValid()
    {
        return gameObject != null && gameObject.IsValid() && !gameObject.IsDead && gameObject.IsTargetable;
    }

    public Vector3 GetPosition()
    {
        return gameObject.Position;
    }

    public uint? GetModelId()
    {
        return GetData()?.SGB.RowId;
    }

    public TreasureType GetTreasureType()
    {
        switch (GetModelId() ?? 0)
        {
            case 1597:
                return TreasureType.Silver;
            case 1596:
                return TreasureType.Bronze;
            default:
                return TreasureType.Unknown;
        }
    }

    public Vector4 GetColor()
    {
        switch (GetTreasureType())
        {
            case TreasureType.Bronze:
                return TreasureModule.bronze;
            case TreasureType.Silver:
                return TreasureModule.silver;
            default:
                return TreasureModule.unknown;
        }
    }

    public string GetName()
    {
        switch (GetTreasureType())
        {
            case TreasureType.Bronze:
                return "Bronze Treasure Coffer";
            case TreasureType.Silver:
                return "Silver Treasure Coffer";
            default:
                return "Unknown Treasure Coffer";
        }
    }

    public void Target()
    {
        Svc.Targets.Target = gameObject;
    }
}
