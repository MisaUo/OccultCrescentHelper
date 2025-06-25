using ImGuiNET;
using Ocelot;

namespace OccultCrescentHelper.Modules.Automator;

public class Panel
{
    public void Draw(AutomatorModule module)
    {
        OcelotUI.Title($"{module.T("panel.title")}:");
        OcelotUI.Indent(() =>
        {
            OcelotUI.Title($"{module.T("panel.activity.label")}:");
            ImGui.SameLine();
            ImGui.TextUnformatted(module.automator.activity?.data.Name ?? module.T("panel.activity.none"));

            OcelotUI.Title($"{module.T("panel.activity_state.label")}:");
            ImGui.SameLine();
            ImGui.TextUnformatted(module.automator.activity?.state.ToLabel() ?? module.T("panel.activity_state.none"));
        });
    }
}
