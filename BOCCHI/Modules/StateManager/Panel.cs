using ImGuiNET;
using Ocelot;

namespace BOCCHI.Modules.StateManager;

public class Panel
{
    public bool Draw(StateManagerModule module)
    {
        if (!module.Config.ShowDebug)
        {
            return false;
        }

        OcelotUI.Title($"{module.T("panel.title")}:");
        OcelotUI.Indent(() => ImGui.TextUnformatted($"{module.T("panel.state.label")}: {module.GetStateText()}"));

        return true;
    }
}
