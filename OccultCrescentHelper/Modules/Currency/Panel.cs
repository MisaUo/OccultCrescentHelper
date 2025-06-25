using Dalamud.Interface;
using ECommons.ImGuiMethods;
using ImGuiNET;
using Ocelot;

namespace BOCCHI.Modules.Currency;

public class Panel
{
    public void Draw(CurrencyModule module)
    {
        OcelotUI.Title($"{module.T("panel.title")}:");
        OcelotUI.Indent(() =>
        {
            // Content here
            if (ImGui.BeginTable("CurrencyData##OCH", 3, ImGuiTableFlags.SizingFixedFit))
            {
                // Silver
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Redo, "Reset##Silver"))
                {
                    module.tracker.ResetSilver();
                }

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(module.T("panel.silver.label"));

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(module.tracker.GetSilverPerHour().ToString("F2"));

                // Gold
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Redo, "Reset##Gold"))
                {
                    module.tracker.ResetGold();
                }

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(module.T("panel.gold.label"));

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(module.tracker.GetGoldPerHour().ToString("F2"));

                ImGui.EndTable();
            }
        });
    }
}
