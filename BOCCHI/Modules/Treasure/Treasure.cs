﻿using BOCCHI.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System.Linq;
using System.Numerics;
using TreasureFlags = FFXIVClientStructs.FFXIV.Client.Game.Object.Treasure.TreasureFlags;
using XIVTreasure = Lumina.Excel.Sheets.Treasure;

namespace BOCCHI.Modules.Treasure;

public class Treasure(IGameObject obj)
{
    public uint Id
    {
        get => obj.DataId;
    }

    private TreasureFlags LastFlags = TreasureFlags.None;

    public unsafe bool CheckOpened()
    {
        var gameObject = (GameObject*)(void*)obj.Address;
        if (gameObject == null)
        {
            return false;
        }

        var instance = (FFXIVClientStructs.FFXIV.Client.Game.Object.Treasure*)gameObject;
        var currentFlags = instance->Flags;

        if (currentFlags != LastFlags)
        {
            var wasNotOpened = !LastFlags.HasFlag(TreasureFlags.Opened);
            var isNowOpened = currentFlags.HasFlag(TreasureFlags.Opened);

            LastFlags = currentFlags;

            if (wasNotOpened && isNowOpened)
            {
                return true;
            }
        }

        return false;
    }


    private XIVTreasure? GetData()
    {
        return Svc.Data.GetExcelSheet<XIVTreasure>().ToList().FirstOrDefault(t => t.RowId == obj.DataId);
    }

    public bool IsValid()
    {
        return obj.IsValid() && obj is { IsDead: false, IsTargetable: true };
    }

    public Vector3 GetPosition()
    {
        return obj.Position;
    }

    private uint? GetModelId()
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
        return GetTreasureType() switch
        {
            TreasureType.Bronze => TreasureModule.Bronze,
            TreasureType.Silver => TreasureModule.Silver,
            _ => TreasureModule.Unknown,
        };
    }

    public string GetName()
    {
        return GetTreasureType() switch
        {
            TreasureType.Bronze => "铜宝箱",
            TreasureType.Silver => "银宝箱",
            _ => "未知宝箱",
        };
    }
}
