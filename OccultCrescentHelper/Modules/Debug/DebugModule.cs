using System.Collections.Generic;
using Dalamud.Plugin.Services;
using ECommons;
using ImGuiNET;
using OccultCrescentHelper.Modules.Debug.Panels;
using Ocelot;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.Debug;

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
    };

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
        foreach (var panel in panels)
        {
            if (ImGui.CollapsingHeader(panel.GetName()))
            {
                panel.Draw(this);
                OcelotUI.VSpace();
            }
        }
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
