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
            var isNearKnowledgeCrystal = ZoneHelper.IsNearKnowledgeCrystal();
            var isQueued = module.buffs.IsQueued();

            if (ImGuiEx.IconButton(FontAwesomeIcon.Redo, "Button##ApplyBuffs", enabled: isNearKnowledgeCrystal && !isQueued))
            {
                module.buffs.QueueBuffs();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(module.T("panel.button.tooltip"));
            }
        });
    }
}
