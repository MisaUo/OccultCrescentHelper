using BOCCHI.Data;
using ImGuiNET;
using Ocelot.Windows;

namespace BOCCHI.Modules.Debug;

#if DEBUG_BUILD
[OcelotWindow]
#endif
public class DebugWindow(Plugin plugin, Config config) : OcelotWindow(plugin, config)
{
    public override void Draw()
    {
        if (!ZoneData.IsInOccultCrescent())
        {
            ImGui.TextUnformatted("Not in Occult Crescent zone.");
            return;
        }

        if (plugin.modules.TryGetModule<DebugModule>(out var module) && module != null)
        {
            module.DrawPanels();
        }
    }

    protected override string GetWindowName()
    {
        return "OCH Debug";
    }
}
