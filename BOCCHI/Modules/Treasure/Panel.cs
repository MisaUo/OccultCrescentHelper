using System.Numerics;
using ECommons.GameHelpers;
using ImGuiNET;
using Ocelot;

namespace BOCCHI.Modules.Treasure;

public class Panel
{
    public void Draw(TreasureModule module)
    {
        OcelotUI.Title($"{module.T("panel.title")}:");

        OcelotUI.Indent(() =>
        {
            DrawActiveChests(module);

            if (module.Treasures.Count <= 0)
            {
                ImGui.TextUnformatted(module.T("panel.none"));
                return;
            }

            foreach (var treasure in module.Treasures)
            {
                if (!treasure.IsValid())
                {
                    continue;
                }

                var pos = treasure.GetPosition();

                ImGui.TextUnformatted($"{treasure.GetName()}");
                OcelotUI.Indent(() =>
                {
                    ImGui.TextUnformatted($"({pos.X:F2}, {pos.Y:F2}, {pos.Z:F2})");
                    ImGui.TextUnformatted($"({Vector3.Distance(Player.Position, pos)})");
                });
            }
        });
    }

    private void DrawActiveChests(TreasureModule module)
    {
        if (!module.Tracker.CountInitialised)
        {
            return;
        }

        OcelotUI.LabelledValue(module.T("panel.active_bronze.label"), $"{module.Tracker.BronzeChests}/30");
        if (module.config.ShowPercentageActiveTreasureCount)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted($"({module.Tracker.BronzeChests / 30f * 100f}%)");
        }

        OcelotUI.LabelledValue(module.T("panel.active_silver.label"), $"{module.Tracker.SilverChests}/8");
        if (module.config.ShowPercentageActiveTreasureCount)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted($"({module.Tracker.SilverChests / 8f * 100f}%)");
        }
    }
}
