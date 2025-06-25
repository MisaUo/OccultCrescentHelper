using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using OccultCrescentHelper.Data;
using OccultCrescentHelper.Modules.Automator;
using Ocelot.Windows;

namespace OccultCrescentHelper.Windows;

[OcelotMainWindow]
public class MainWindow : OcelotMainWindow
{
    public MainWindow(Plugin plugin, Config config)
        : base(plugin, config)
    {
    }

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

                plugin.modules.GetModule<AutomatorModule>().DisableIllegalMode();
            },
            Icon = FontAwesomeIcon.Stop,
            IconOffset = new Vector2(2, 2),
            ShowTooltip = () => ImGui.SetTooltip("Emergency Stop"),
        });

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

        plugin.modules?.DrawMainUi();
    }
}
