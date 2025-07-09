using System.Collections.Generic;
using System.Numerics;
using BOCCHI.Modules.Debug.Panels;
using Dalamud.Plugin.Services;
using ECommons;
using ImGuiNET;
using Ocelot.Modules;

namespace BOCCHI.Modules.Debug;

#if DEBUG_BUILD
[OcelotModule]
#endif
public class DebugModule : Module<Plugin, Config>
{
    private List<Panel> panels = new()
    {
        new TeleporterPanel(),
        new VnavmeshPanel(),
        new FatesPanel(),
        new CriticalEncountersPanel(),
        new ChainManagerPanel(),
        new EnemyPanel(),
        new StatusPanel(),
        new TargetPanel(),
        new ActivityTargetPanel(),
        new TreasureHuntPanel(),
        new CarrotHuntPanel(),
        new JobLevelPanel(),
    };

    private int selectedPanelIndex = 0;

    public DebugModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
    }

    public override void PostInitialize()
    {
        if (plugin.windows.TryGetWindow<DebugWindow>(out var window) && window != null && !window.IsOpen)
        {
            window.Toggle();
        }
    }

    public void DrawPanels()
    {
        // Determine sizes
        var panelWidth = 200f;
        var spacing = ImGui.GetStyle().ItemSpacing.X;

        ImGui.BeginGroup();

        // Left panel list
        ImGui.BeginChild("PanelList", new Vector2(panelWidth, 0), true);
        for (var i = 0; i < panels.Count; i++)
        {
            var selected = i == selectedPanelIndex;
            if (ImGui.Selectable(panels[i].GetName(), selected))
            {
                selectedPanelIndex = i;
            }
        }

        ImGui.EndChild();

        ImGui.SameLine(0, spacing);

        // Right panel content
        ImGui.BeginGroup();
        ImGui.BeginChild("PanelContent", new Vector2(0, 0), false);
        panels[selectedPanelIndex].Draw(this);
        ImGui.EndChild();
        ImGui.EndGroup();

        ImGui.EndGroup();
    }

    public override void Tick(IFramework _)
    {
        panels.Each(p => p.Tick(this));
    }

    public override void OnTerritoryChanged(ushort id)
    {
        panels.Each(p => p.OnTerritoryChanged(id, this));
    }
}
