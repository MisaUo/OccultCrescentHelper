using Dalamud.Interface;
using ImGuiNET;
using OccultCrescentHelper.Data;
using Ocelot.Windows;

namespace OccultCrescentHelper.Modules.Automator;

[OcelotWindow]
public class AutomatorWindow : OcelotWindow
{
    public AutomatorWindow(Plugin plugin, Config config)
        : base(plugin, config, "OCH Automator Lens")
    {
        TitleBarButtons.Add(new() {
            Click = (m) => {
                if (m != ImGuiMouseButton.Left)
                {
                    return;
                }

                AutomatorModule.ToggleIllegalMode(plugin);
            },
            Icon = FontAwesomeIcon.Skull,
            IconOffset = new(2, 2),
            ShowTooltip = () => ImGui.SetTooltip("Toggle Illegal Mode"),
        });
    }

    public override void Draw()
    {
        if (!ZoneData.IsInOccultCrescent())
        {
            ImGui.TextUnformatted("Not in Occult Crescent zone.");
            return;
        }

        var automator = plugin.modules.GetModule<AutomatorModule>();
        if (automator == null || !automator.enabled)
        {
            ImGui.TextUnformatted("Automator is not enabled.");
            return;
        }

        automator.panel.Draw(automator);
    }
}
