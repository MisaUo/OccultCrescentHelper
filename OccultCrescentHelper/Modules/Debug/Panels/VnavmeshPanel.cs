using ImGuiNET;
using Ocelot;
using Ocelot.IPC;

namespace OccultCrescentHelper.Modules.Debug.Panels;

public class VnavmeshPanel : Panel
{
    public override string GetName()
    {
        return "Vnavmesh";
    }

    public override void Draw(DebugModule module)
    {
        if (module.TryGetIPCProvider<VNavmesh>(out var vnav) && vnav!.IsReady())
        {
            OcelotUI.Title("Vnav state:");
            ImGui.SameLine();
            ImGui.TextUnformatted(vnav.IsRunning() ? "Running" : "Pending");
        }
    }
}
