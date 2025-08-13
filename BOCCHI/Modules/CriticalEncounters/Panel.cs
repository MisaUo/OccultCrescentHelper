﻿using BOCCHI.Data;
using BOCCHI.Modules.Teleporter;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using ImGuiNET;
using Ocelot;
using System;
using System.Linq;

namespace BOCCHI.Modules.CriticalEncounters;

public class Panel
{
    public void Draw(CriticalEncountersModule module)
    {
        OcelotUI.Title($"{module.T("panel.title")}:");
        OcelotUI.Indent(() =>
        {
            var active = module.CriticalEncounters.Values.Count(ev => ev.State != DynamicEventState.Inactive);
            if (active <= 0)
            {
                ImGui.TextUnformatted(module.T("panel.none"));
                return;
            }

            foreach (var ev in module.CriticalEncounters.Values)
            {
                if (!ZoneData.IsInOccultCrescent())
                {
                    module.CriticalEncounters.Clear();
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

                            if (module.Progress.TryGetValue(ev.DynamicEventId, out var progress))
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
                    OcelotUI.Indent(() => EventIconRenderer.Drops(data, module.PluginConfig.EventDropConfig));
                    continue;
                }

                if (module.TryGetModule<TeleporterModule>(out var teleporter) && teleporter!.IsReady())
                {
                    var start = ev.MapMarker.Position;

                    teleporter.teleporter.Button(data.Aethernet, start, ev.Name.ToString(), $"ce_{ev.DynamicEventId}", data);
                }

                OcelotUI.Indent(() => EventIconRenderer.Drops(data, module.PluginConfig.EventDropConfig));
            }
        });
    }


    private void HandleTower(DynamicEvent ev, CriticalEncountersModule module)
    {
        if (!module.Config.TrackForkedTower || ev.State == DynamicEventState.Battle)
        {
            return;
        }

        OcelotUI.Error("此功能尚在开发中");

        if (ev.State == DynamicEventState.Inactive)
        {
            ImGui.TextUnformatted($"{ev.Name}:");

            var time = module.Tracker.TowerTimer.GetTimeToForkedTowerSpawn(ev.State);
            OcelotUI.Indent(() => { OcelotUI.LabelledValue("两歧塔出现预计还需", $"{time:mm\\:ss}"); });
        }
        else
        {
            ImGui.TextUnformatted($"{ev.Name}:");

            var time = module.Tracker.TowerTimer.GetTimeRemainingToRegister(ev.State);
            OcelotUI.Indent(() => { OcelotUI.LabelledValue("两歧塔报名时间", $"{time:mm\\:ss}"); });
        }

        OcelotUI.Indent(32, () =>
        {
            OcelotUI.LabelledValue("紧急遭遇战已完成", module.Tracker.TowerTimer.CriticalEncountersCompleted);
            OcelotUI.LabelledValue("FATE已完成", module.Tracker.TowerTimer.FatesCompleted);
        });


        if (!TowerHelper.IsPlayerNearTower(TowerHelper.TowerType.Blood))
        {
            return;
        }

        OcelotUI.Indent(() =>
        {
            OcelotUI.LabelledValue("平台上的玩家", TowerHelper.GetPlayersInTowerZone(TowerHelper.TowerType.Blood));
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("包括你的角色");
            }

            OcelotUI.LabelledValue("平台附近的玩家", TowerHelper.GetPlayersNearTowerZone(TowerHelper.TowerType.Blood));
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("包括你的角色");
            }
        });
    }
}
