using ECommons.ImGuiMethods;
using ImGuiNET;
using Ocelot;

namespace OccultCrescentHelper.Modules.Exp;

public class Panel
{
    public void Draw(ExpModule module)
    {
        OcelotUI.Title($"{module.T("panel.title")}:");
        OcelotUI.Indent(() =>
        {
            if (ImGuiEx.IconButton(Dalamud.Interface.FontAwesomeIcon.Redo, $"Reset##Exp"))
            {
                module.tracker.Reset();
            }

            ImGui.SameLine();
            ImGui.TextUnformatted(module.T("panel.exp.label"));

            ImGui.SameLine();
            ImGui.TextUnformatted(module.tracker.GetExpPerHour().ToString("F2"));
        });
    }
}
