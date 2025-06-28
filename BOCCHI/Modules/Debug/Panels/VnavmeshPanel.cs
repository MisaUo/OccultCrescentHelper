using System.Numerics;
using ImGuiNET;
using Ocelot;
using Ocelot.IPC;

namespace BOCCHI.Modules.Debug.Panels;

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


            if (ImGui.Button("Test vnav thingy"))
            {
                vnav.MoveToPath([new Vector3(815.2f, 72.5f, -705.15f)], false);
            }
        }
    }
}
