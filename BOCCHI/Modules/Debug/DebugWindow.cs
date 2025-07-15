using BOCCHI.Data;
using ImGuiNET;
using Ocelot;
using Ocelot.Windows;

namespace BOCCHI.Modules.Debug;

#if DEBUG_BUILD
[OcelotWindow]
#endif
public class DebugWindow(Plugin priamryPlugin, Config config) : OcelotWindow(priamryPlugin, config)
{
    protected override void Render(RenderContext context)
    {
        if (!ZoneData.IsInOccultCrescent())
        {
            ImGui.TextUnformatted(I18N.T("generic.label.not_in_zone"));
            return;
        }

        if (plugin.Modules.TryGetModule<DebugModule>(out var module) && module != null)
        {
            module.DrawPanels();
        }
    }

    protected override string GetWindowName()
    {
        return "OCH Debug";
    }
}
