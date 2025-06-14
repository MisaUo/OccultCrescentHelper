using ImGuiNET;
using Ocelot;

namespace OccultCrescentHelper.Modules.Carrots;


public class Panel
{
    public void Draw(CarrotsModule module)
    {
        OcelotUI.Title($"{module.T("panel.title")}:");
        OcelotUI.Indent(() => {
            if (module.carrots.Count <= 0)
            {
                ImGui.TextUnformatted(module.T("panel.none"));
                return;
            }

            foreach (var carrot in module.carrots)
            {
                if (!carrot.IsValid())
                {
                    continue;
                }

                var pos = carrot.GetPosition();
                ImGui.TextUnformatted($"{module.T("panel.label")}: ({pos.X:F2}, {pos.Y:F2}, {pos.Z:F2})");
            }
        });
    }
}
