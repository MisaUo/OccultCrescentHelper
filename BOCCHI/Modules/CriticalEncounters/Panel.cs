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
        OcelotUI.Indent(() =>
        {
            var active = module.criticalEncounters.Values.Count(ev => ev.State != DynamicEventState.Inactive);
            if (active <= 0)
            {
                ImGui.TextUnformatted(module.T("panel.none"));
                return;
            }

            foreach (var ev in module.criticalEncounters.Values)
            {
                if (!ZoneData.IsInOccultCrescent())
                {
                    module.criticalEncounters.Clear();
                    return;
                }

                if (ev.EventType >= 4)
                {
                    HandleTower(ev, module);
                    continue;
                }

                if (ev.State == DynamicEventState.Inactive)
                {
                    continue;
                }

                if (!EventData.CriticalEncounters.TryGetValue(ev.DynamicEventId, out var data))
                {
                    continue;
                }

                ImGui.TextUnformatted(ev.Name.ToString());

                switch (ev.State)
                {
                    case DynamicEventState.Register:
                        {
                            var start = DateTimeOffset.FromUnixTimeSeconds(ev.StartTimestamp).DateTime;
                            var timeUntilStart = start - DateTime.UtcNow;
                            var formattedTime = $"{timeUntilStart.Minutes:D2}:{timeUntilStart.Seconds:D2}";

                            ImGui.SameLine();
                            ImGui.TextUnformatted($"({module.T("panel.register")}: {formattedTime})");
                            break;
                        }
                    case DynamicEventState.Warmup:
                        ImGui.SameLine();
                        ImGui.TextUnformatted($"({module.T("panel.warmup")})");
                        break;
                    case DynamicEventState.Battle:
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

                            break;
                        }
                    case DynamicEventState.Inactive:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (ev.State != DynamicEventState.Register)
                {
                    OcelotUI.Indent(() => EventIconRenderer.Drops(data, module.plugin.config.EventDropConfig));
                    continue;
                }

                if (module.TryGetModule<TeleporterModule>(out var teleporter) && teleporter!.IsReady())
                {
                    var start = ev.MapMarker.Position;

                    teleporter.teleporter.Button(data.aethernet, start, ev.Name.ToString(), $"ce_{ev.DynamicEventId}", data);
                }

                OcelotUI.Indent(() => EventIconRenderer.Drops(data, module.plugin.config.EventDropConfig));
            }
        });
    }


    private void HandleTower(DynamicEvent ev, CriticalEncountersModule module)
    {
        if (!module.config.TrackForkedTower || ev.State == DynamicEventState.Battle)
        {
            return;
        }

        OcelotUI.Error("This feature is a work in progress.");

        if (ev.State == DynamicEventState.Inactive)
        {
            ImGui.TextUnformatted($"{ev.Name}:");

            var time = module.tracker.TowerTimer.GetTimeToForkedTowerSpawn(ev.State);
            OcelotUI.Indent(() => { OcelotUI.LabelledValue("Forked Tower Spawn Estimate", $"{time:mm\\:ss}"); });
        }
        else
        {
            ImGui.TextUnformatted($"{ev.Name}:");

            var time = module.tracker.TowerTimer.GetTimeRemainingToRegister(ev.State);
            OcelotUI.Indent(() => { OcelotUI.LabelledValue("Forked Tower Register", $"{time:mm\\:ss}"); });
        }

        OcelotUI.Indent(32, () =>
        {
            OcelotUI.LabelledValue("Critical Encounters completed", module.tracker.TowerTimer.CriticalEncountersCompleted);
            OcelotUI.LabelledValue("Fates completed", module.tracker.TowerTimer.FatesCompleted);
        });


        if (!TowerHelper.IsPlayerNearTower(TowerHelper.TowerType.Blood))
        {
            return;
        }

        OcelotUI.Indent(() =>
        {
            OcelotUI.LabelledValue("Players on Platform", TowerHelper.GetPlayersInTowerZone(TowerHelper.TowerType.Blood));
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("This includes your character");
            }

            OcelotUI.LabelledValue("Players near Platform", TowerHelper.GetPlayersNearTowerZone(TowerHelper.TowerType.Blood));
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("This includes your character");
            }
        });
    }
}
