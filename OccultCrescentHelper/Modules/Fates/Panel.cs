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
            if (module.tracker.fates.Count <= 0)
            {
                ImGui.TextUnformatted(module.T("panel.none"));
                return;
            }

            foreach (var fate in module.fates.Values)
            {
                if (fate == null)
                {
                    continue;
                }

                if (!EventData.Fates.TryGetValue(fate.FateId, out var data))
                {
                    continue;
                }

                ImGui.TextUnformatted($"{data.Name} ({fate.Progress}%)");


                if (module.progress.TryGetValue(fate.FateId, out var progress))
                {
                    var estimate = progress.EstimateTimeToCompletion();
                    if (estimate != null)
                    {
                        ImGui.SameLine();
                        ImGui.TextUnformatted($"({module.T("panel.estimated")} {estimate.Value:mm\\:ss})");
                    }
                }

                if (module.TryGetModule<TeleporterModule>(out var teleporter) && teleporter!.IsReady())
                {
                    teleporter.teleporter.Button(data.aethernet, data.start ?? fate.Position, data.Name,
                        $"fate_{fate.FateId}", data);
                }

                OcelotUI.Indent(() => EventIconRenderer.Drops(data, module.plugin.config.EventDropConfig));

                if (!fate.Equals(module.fates.Values.Last()))
                {
                    OcelotUI.VSpace();
                }
            }
        });
    }
}
