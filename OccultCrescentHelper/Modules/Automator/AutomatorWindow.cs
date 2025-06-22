using Dalamud.Interface;
using ImGuiNET;
using OccultCrescentHelper.Data;
using Ocelot.Windows;

namespace OccultCrescentHelper.Modules.Automator;

[OcelotWindow]
public class AutomatorWindow : OcelotWindow
{
    public AutomatorWindow(Plugin plugin, Config config)
        : base(plugin, config, "OCH Illegal Lens")
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
            ImGui.TextUnformatted("Illegal Mode is not enabled.");
            return;
        }

        automator.panel.Draw(automator);
    }
}
