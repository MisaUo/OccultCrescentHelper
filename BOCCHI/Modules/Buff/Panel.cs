using BOCCHI.Data;
using Dalamud.Interface;
using ECommons.ImGuiMethods;
using ImGuiNET;
using Ocelot;

namespace BOCCHI.Modules.Buff;

public class Panel
{
    public void Draw(BuffModule module)
    {
        OcelotUI.Title($"{module.T("panel.title")}:");
        OcelotUI.Indent(() =>
        {
            var isNearKnowledgeCrystal = ZoneData.IsNearKnowledgeCrystal();
            var isQueued = module.BuffManager.IsQueued();

            if (ImGuiEx.IconButton(FontAwesomeIcon.Redo, "Button##ApplyBuffs", enabled: isNearKnowledgeCrystal && !isQueued))
            {
                module.BuffManager.QueueBuffs();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(module.T("panel.button.tooltip"));
            }
        });
    }
}
