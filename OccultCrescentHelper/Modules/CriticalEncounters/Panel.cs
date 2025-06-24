using System;
using System.Linq;
using BOCCHI.Data;
using BOCCHI.Modules.Teleporter;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using ImGuiNET;
using Ocelot;

namespace BOCCHI.Modules.CriticalEncounters;

public class Panel
{
    public void Draw(CriticalEncountersModule module)
    {
        OcelotUI.Title($"{module.T("panel.title")}:");
        OcelotUI.Indent(() => {
            var active = module.criticalEncounters.Values.Where(ev => ev.State != DynamicEventState.Inactive).Count();
            if (active <= 0)
            {
                ImGui.TextUnformatted(module.T("panel.none"));
                return;
            }

            foreach (var ev in module.criticalEncounters.Values)
            {
                if (ev.State == DynamicEventState.Inactive) continue;

                if (!EventData.CriticalEncounters.TryGetValue(ev.DynamicEventId, out var data)) continue;

                ImGui.TextUnformatted(ev.Name.ToString());
                if (ev.EventType >= 4)
                {
                    HandlerTower(ev);
                    continue;
                }

                if (ev.State == DynamicEventState.Register)
                {
                    var start = DateTimeOffset.FromUnixTimeSeconds(ev.StartTimestamp).DateTime;
                    var timeUntilStart = start - DateTime.UtcNow;
                    var formattedTime = $"{timeUntilStart.Minutes:D2}:{timeUntilStart.Seconds:D2}";

                    ImGui.SameLine();
                    ImGui.TextUnformatted($"({module.T("panel.register")}: {formattedTime})");
                }

                if (ev.State == DynamicEventState.Warmup)
                {
                    ImGui.SameLine();
                    ImGui.TextUnformatted($"({module.T("panel.warmup")})");
                }

                if (ev.State == DynamicEventState.Battle)
                {
                    ImGui.SameLine();
                    ImGui.TextUnformatted($"({ev.Progress}%)");

                    if (module.progress.TryGetValue(ev.DynamicEventId, out var progress))
                    {
                        var estimate = progress.EstimateTimeToCompletion();
                        if (estimate != null)
                        {
                            ImGui.SameLine();
                            ImGui.TextUnformatted($"({module.T("panel.estimated")} {estimate.Value:mm\\:ss})");
                        }
                    }
                }

                if (ev.State != DynamicEventState.Register)
                {
                    OcelotUI.Indent(() => EventIconRenderer.Drops(data, module.plugin.Config.EventDropConfig));
                    continue;
                }

                if (module.TryGetModule<TeleporterModule>(out var teleporter) && teleporter!.IsReady())
                {
                    var start = ev.MapMarker.Position;

                    teleporter.teleporter.Button(data.aethernet, start, data.Name, $"ce_{ev.DynamicEventId}", data);
                }

                OcelotUI.Indent(() => EventIconRenderer.Drops(data, module.plugin.Config.EventDropConfig));
            }
        });
    }


    private void HandlerTower(DynamicEvent ev) { }
}
