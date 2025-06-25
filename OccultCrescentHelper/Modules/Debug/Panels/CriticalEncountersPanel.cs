using System;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using OccultCrescentHelper.Data;
using OccultCrescentHelper.Modules.CriticalEncounters;
using OccultCrescentHelper.Modules.Teleporter;
using Ocelot;

namespace OccultCrescentHelper.Modules.Debug.Panels;

public class CriticalEncountersPanel : Panel
{
    public override string GetName()
    {
        return "Critical Encounters";
    }

    public override unsafe void Draw(DebugModule module)
    {
        OcelotUI.Title("Critical Encounters:");
        OcelotUI.Indent(() => {
            foreach (var data in EventData.CriticalEncounters.Values)
            {
                var ev = module.GetModule<CriticalEncountersModule>().criticalEncounters[data.id];

                ImGui.TextUnformatted(ev.Name.ToString());

                if (ev.State == DynamicEventState.Inactive)
                {
                    ImGui.SameLine();
                    ImGui.TextUnformatted($"(Inactive)");
                }

                if (ev.State == DynamicEventState.Register)
                {
                    var start = DateTimeOffset.FromUnixTimeSeconds(ev.StartTimestamp).DateTime;
                    var timeUntilStart = start - DateTime.UtcNow;
                    var formattedTime = $"{timeUntilStart.Minutes:D2}:{timeUntilStart.Seconds:D2}";

                    ImGui.SameLine();
                    ImGui.TextUnformatted($"(Preparing: {formattedTime})");
                }

                if (ev.State == DynamicEventState.Warmup)
                {
                    ImGui.SameLine();
                    ImGui.TextUnformatted($"(Starting)");
                }

                if (ev.State == DynamicEventState.Battle)
                {
                    ImGui.SameLine();
                    ImGui.TextUnformatted($"({ev.Progress}%)");
                }

                if (module.TryGetModule<TeleporterModule>(out var teleporter) && teleporter!.IsReady())
                {
                    var start = ev.MapMarker.Position;

                    teleporter.teleporter.Button(data.aethernet, start, data.Name, $"ce_{data.id}", data);
                }

                OcelotUI.Indent(() => EventIconRenderer.Drops(data, module.plugin.config.EventDropConfig));

                if (data.pathFactory != null)
                {
                    ImGui.TextColored(new Vector4(0.5f, 1.0f, 0.5f, 1.0f), "Has custom path");
                }

                if (data.id != EventData.CriticalEncounters.Keys.Max())
                {
                    OcelotUI.VSpace();
                }

                if (ImGui.CollapsingHeader($"Event Data##{data.id}"))
                {
                    PrintEvent(ev);
                }

                if (ImGui.CollapsingHeader($"Map Marker Data##{data.id}"))
                {
                    PrintMapMarker(ev.MapMarker);
                }
            }
        });
    }

    private unsafe void PrintEvent(DynamicEvent ev)
    {
        OcelotUI.Title("Name Offset:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.NameOffset.ToString());

        OcelotUI.Title("Description Offset:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.DescriptionOffset.ToString());

        OcelotUI.Title("LGB Event Object:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.LGBEventObject.ToString());

        OcelotUI.Title("LGB Map Range:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.LGBMapRange.ToString());

        OcelotUI.Title("Quest (RowId):");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Quest.ToString());

        OcelotUI.Title("Announce (RowId):");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Announce.ToString());

        OcelotUI.Title("Unknown0:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Unknown0.ToString());

        OcelotUI.Title("Unknown1:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Unknown1.ToString());

        OcelotUI.Title("Unknown6:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Unknown6.ToString());

        OcelotUI.Title("Unknown7:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Unknown7.ToString());

        OcelotUI.Title("Unknown2:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Unknown2.ToString());

        OcelotUI.Title("Event Type (RowId):");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.EventType.ToString());

        OcelotUI.Title("Enemy Type (RowId):");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.EnemyType.ToString());

        OcelotUI.Title("Max Participants:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.MaxParticipants.ToString());

        OcelotUI.Title("Radius?:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Unknown4.ToString());

        OcelotUI.Title("Unknown5:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Unknown5.ToString());

        OcelotUI.Title("Single Battle (RowId):");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.SingleBattle.ToString());

        OcelotUI.Title("Unknown8:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Unknown8.ToString());

        OcelotUI.Title("Start Timestamp:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.StartTimestamp.ToString());

        OcelotUI.Title("Seconds Left:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.SecondsLeft.ToString());

        OcelotUI.Title("Seconds Duration:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.SecondsDuration.ToString());

        OcelotUI.Title("Dynamic Event Id:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.DynamicEventId.ToString());

        OcelotUI.Title("Dynamic Event Type:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.DynamicEventType.ToString());

        OcelotUI.Title("State:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.State.ToString());

        OcelotUI.Title("Participants:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Participants.ToString());

        OcelotUI.Title("Progress:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Progress.ToString());

        OcelotUI.Title("Name:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Name.ToString());

        OcelotUI.Title("Description:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.Description.ToString());

        OcelotUI.Title("Icon Objective 0:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.IconObjective0.ToString());

        OcelotUI.Title("Max Participants 2:");
        ImGui.SameLine();
        ImGui.TextUnformatted(ev.MaxParticipants2.ToString());

        OcelotUI.Title("Map Marker:");
        ImGui.SameLine();
        ImGui.TextUnformatted($"X: {ev.MapMarker.X}, Y: {ev.MapMarker.Y}, IconId: {ev.MapMarker.IconId}"); // example, adjust fields accordingly

        OcelotUI.Title("Event Container Pointer:");
        ImGui.SameLine();
        ImGui.TextUnformatted(((IntPtr)ev.EventContainer).ToString("X"));
    }


    private unsafe void PrintMapMarker(MapMarkerData marker)
    {
        OcelotUI.Title("Level Id:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.LevelId.ToString());

        OcelotUI.Title("Objective Id:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.ObjectiveId.ToString());

        OcelotUI.Title("Tooltip String:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.TooltipString != null ? marker.TooltipString->ToString() : "null");

        OcelotUI.Title("Icon Id:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.IconId.ToString());

        OcelotUI.Title("Position X:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.Position.X.ToString("F2"));

        OcelotUI.Title("Position Y:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.Position.Y.ToString("F2"));

        OcelotUI.Title("Position Z:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.Position.Z.ToString("F2"));

        OcelotUI.Title("Radius:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.Radius.ToString("F2"));

        OcelotUI.Title("Map Id:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.MapId.ToString());

        OcelotUI.Title("Place Name Zone Id:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.PlaceNameZoneId.ToString());

        OcelotUI.Title("Place Name Id:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.PlaceNameId.ToString());

        OcelotUI.Title("End Timestamp:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.EndTimestamp.ToString());

        OcelotUI.Title("Recommended Level:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.RecommendedLevel.ToString());

        OcelotUI.Title("Territory Type Id:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.TerritoryTypeId.ToString());

        OcelotUI.Title("Data Id:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.DataId.ToString());

        OcelotUI.Title("Marker Type:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.MarkerType.ToString());

        OcelotUI.Title("Event State:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.EventState.ToString());

        OcelotUI.Title("Flags:");
        ImGui.SameLine();
        ImGui.TextUnformatted(marker.Flags.ToString());
    }
}
