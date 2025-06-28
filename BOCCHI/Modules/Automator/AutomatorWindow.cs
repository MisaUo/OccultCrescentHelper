using System.Numerics;
using BOCCHI.Data;
using Dalamud.Interface;
using ImGuiNET;
using Ocelot.Windows;

namespace BOCCHI.Modules.Automator;

[OcelotWindow]
public class AutomatorWindow(Plugin priamryPlugin, Config config) : OcelotWindow(priamryPlugin, config)
{
    public override void PostInitialize()
    {
        base.PostInitialize();

        TitleBarButtons.Add(new TitleBarButton
        {
            Click = (m) =>
            {
                if (m != ImGuiMouseButton.Left)
                {
                    return;
                }

                AutomatorModule.ToggleIllegalMode(plugin);
            },
            Icon = FontAwesomeIcon.Skull,
            IconOffset = new Vector2(2, 2),
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

    protected override string GetWindowName()
    {
        // @todo: Localize this
        return "BOCCHI Illegal Lens";
    }
}
