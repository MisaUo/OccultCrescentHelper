using BOCCHI.Data;
using ImGuiNET;
using Ocelot;

namespace BOCCHI.Modules.ForkedTower;

public class Panel
{
    public void Draw(ForkedTowerModule module)
    {
        if (!ZoneData.IsInForkedTower())
        {
            return;
        }

        OcelotUI.Title("Forked Tower:");
        OcelotUI.Indent(() =>
        {
            var state = OcelotUI.LabelledValue("Tower ID", module.TowerRun.Hash);
            if (state == UIState.Hovered)
            {
                ImGui.SetTooltip("This is unique to you.");
            }
        });
    }
}
