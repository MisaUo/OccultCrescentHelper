using System.Numerics;
using ECommons.GameHelpers;
using ImGuiNET;
using Ocelot;

namespace OccultCrescentHelper.Modules.Treasure;

public class Panel
{
    public void Draw(TreasureModule module)
    {
        OcelotUI.Title($"{module.T("panel.title")}:");
        OcelotUI.Indent(() =>
        {
            if (module.treasures.Count <= 0)
            {
                ImGui.TextUnformatted(module.T("panel.none"));
                return;
            }

            if (ImGui.BeginTable("Treasure", 2, ImGuiTableFlags.SizingFixedFit))
            {
                var index = 0;
                foreach (var treasure in module.treasures)
                {
                    if (!treasure.IsValid())
                    {
                        continue;
                    }

                    var pos = treasure.GetPosition();

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"{treasure.GetName()}");
                    OcelotUI.Indent(() =>
                    {
                        ImGui.TextUnformatted($"({pos.X:F2}, {pos.Y:F2}, {pos.Z:F2})");
                        ImGui.TextUnformatted($"({Vector3.Distance(Player.Position, pos)})");
                    });

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"Target###{index}"))
                    {
                        treasure.Target();
                    }

                    index++;
                }

                ImGui.EndTable();
            }
        });
    }
}
