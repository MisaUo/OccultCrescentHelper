using System;
using System.Linq;
using BOCCHI.Data;
using BOCCHI.Modules.Teleporter;
using ImGuiNET;
using Ocelot;

namespace BOCCHI.Modules.Fates;

public class Panel
{
    public void Draw(FatesModule module)
    {
        OcelotUI.Title($"{module.T("panel.title")}:");
        OcelotUI.Indent(() =>
        {
            if (module.tracker.Fates.Count <= 0)
            {
                ImGui.TextUnformatted(module.T("panel.none"));
                return;
            }

            foreach (var fate in module.fates.Values)
            {
                if (!ZoneData.IsInOccultCrescent())
                {
                    module.fates.Clear();
                    return;
                }

                try
                {
                    ImGui.TextUnformatted($"{fate.Name} ({fate.CurrentProgress}%)");
                }
                catch (AccessViolationException)
                {
                    continue;
                }


                var estimate = fate.Progress.EstimateTimeToCompletion();
                if (estimate != null)
                {
                    ImGui.SameLine();
                    ImGui.TextUnformatted($"({module.T("panel.estimated")} {estimate.Value:mm\\:ss})");
                }


                if (module.TryGetModule<TeleporterModule>(out var teleporter) && teleporter!.IsReady())
                {
                    teleporter.teleporter.Button(fate.Data.Aethernet, fate.StartPosition, fate.Name, $"fate_{fate.Id}", fate.Data);
                }

                OcelotUI.Indent(() => EventIconRenderer.Drops(fate.Data, module.PluginConfig.EventDropConfig));

                if (!fate.Equals(module.fates.Values.Last()))
                {
                    OcelotUI.VSpace();
                }
            }
        });
    }
}
