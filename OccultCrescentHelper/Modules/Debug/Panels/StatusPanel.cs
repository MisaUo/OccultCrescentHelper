using System.Linq;
using ECommons.DalamudServices;
using ImGuiNET;
using Lumina.Excel.Sheets;
using Ocelot;

namespace OccultCrescentHelper.Modules.Debug.Panels;

public class StatusPanel : Panel
{
    public override string GetName()
    {
        return "Statuses";
    }

    public override void Draw(DebugModule module)
    {
        var data = Svc.Data.GetExcelSheet<Status>();


        OcelotUI.Title("Statuses:");
        OcelotUI.Indent(() => {
            foreach (var s in Svc.ClientState.LocalPlayer!.StatusList)
            {
                ImGui.TextUnformatted($"{data.Where(r => r.RowId == s.StatusId).First().Name} ({s.StatusId})");
            }
        });
    }
}
