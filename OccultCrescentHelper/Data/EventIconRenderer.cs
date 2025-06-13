using System;
using System.Numerics;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Lumina.Excel.Sheets;
using OccultCrescentHelper.Modules.EventDrop;

namespace OccultCrescentHelper.Data;

public struct EventIconRenderer
{
    public static void Drops(EventData data, EventDropConfig config)
    {
        uint rendered = 0;

        if (data.demiatma != null && config.ShowDemiatmaDrops)
        {
            Demiatma(data);
            rendered++;
        }

        if (data.notes != null && config.ShowNoteDrops)
        {
            if (rendered > 0)
            {
                ImGui.SameLine();
            }

            Notes(data);
            rendered++;
        }

        if (data.soulshard != null && config.ShowSoulShardDrops)
        {
            if (rendered > 0)
            {
                ImGui.SameLine();
            }

            SoulShard(data);
            rendered++;
        }
    }

    public static unsafe void Demiatma(EventData data)
    {
        var itemData = Svc.Data.GetExcelSheet<Item>().GetRow((uint)data.demiatma!);

        var count = InventoryManager.Instance()->GetInventoryItemCount(itemData.RowId);
        var needed = Math.Max(0, 3 - count);

        var border = needed > 0 ? new Vector4(0.3f, 0.85f, 0.39f, 1f) : new Vector4(0.95f, 0.26f, 0.21f, 1f);

        var demiatma = Svc.Texture.GetFromGameIcon(new GameIconLookup(itemData.Icon)).GetWrapOrEmpty();

        DrawIcon(demiatma, border, $"Demiatma_{itemData.RowId}_fate_{data.id}");

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();

            var label = $"Needed ({needed})";
            ;
            if (needed <= 0)
            {
                label = $"Not Needed ({count})";
            }

            ImGui.TextUnformatted($"{itemData.Name}: {label}");
            ImGui.EndTooltip();
        }
    }

    public static unsafe void Notes(EventData data)
    {
        var itemData = Svc.Data.GetExcelSheet<Item>().GetRow((uint)data.notes!);

        var notes = Svc.Texture.GetFromGameIcon(new GameIconLookup(itemData.Icon)).GetWrapOrEmpty();

        DrawIcon(notes, new Vector4(1f, 1f, 1f, 1f), $"Note_{itemData.RowId}_fate_{data.id}");

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(itemData.Name.ToString());
            ImGui.EndTooltip();
        }
    }

    private static unsafe void SoulShard(EventData data)
    {
        var itemData = Svc.Data.GetExcelSheet<Item>().GetRow((uint)data.soulshard!);

        var soulshard = Svc.Texture.GetFromGameIcon(new GameIconLookup(itemData.Icon)).GetWrapOrEmpty();

        DrawIcon(soulshard, new Vector4(1f, 1f, 1f, 1f), $"SoulShard_{itemData.RowId}_ce_{data.id}");

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(itemData.Name.ToString());
            ImGui.EndTooltip();
        }
    }

    public static void DrawIcon(IDalamudTextureWrap icon, Vector4 border, string id)
    {
        ImGui.PushStyleColor(ImGuiCol.Border, border);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        ImGui.BeginChild($"ImageBorder##{id}", new Vector2(50, 50), true, ImGuiWindowFlags.NoScrollbar);

        ImGui.Image(icon.ImGuiHandle, new System.Numerics.Vector2(48, 48));

        ImGui.EndChild();

        ImGui.PopStyleVar();
        ImGui.PopStyleColor();
    }
}
