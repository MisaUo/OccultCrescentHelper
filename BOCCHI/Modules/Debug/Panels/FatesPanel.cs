using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BOCCHI.Data;
using BOCCHI.Modules.Teleporter;
using ECommons.DalamudServices;
using ImGuiNET;
using Lumina.Data.Files;
using Lumina.Excel.Sheets;
using Ocelot;

namespace BOCCHI.Modules.Debug.Panels;

public class FatesPanel : Panel
{
    public Dictionary<uint, Vector3> FateLocations = [];

    public FatesPanel()
    {
        ProcessLgbData(Svc.ClientState.TerritoryType);
    }

    public void ProcessLgbData(ushort id)
    {
        if (id == 0)
        {
            return;
        }

        FateLocations.Clear();

        var territorySheet = Svc.Data.GetExcelSheet<TerritoryType>();
        var territoryRow = territorySheet?.GetRow(id);
        if (territoryRow == null)
        {
            Svc.Log.Error($"Could not load TerritoryType for ID {id}");
            return;
        }

        Dictionary<uint, uint> locations = [];
        foreach (var fate in EventData.Fates.Values)
        {
            var fateRow = Svc.Data.GetExcelSheet<Fate>().FirstOrDefault(f => f.RowId == fate.id);
            locations[fate.id] = fateRow.Location;
        }


        var bg = territoryRow?.Bg.ExtractText();
        var lgbFileName = "bg/" + bg![..(bg!.IndexOf("/level/", StringComparison.Ordinal) + 1)] + "level/planevent.lgb";
        var lgb = Svc.Data.GetFile<LgbFile>(lgbFileName);
        foreach (var layer in lgb?.Layers ?? [])
        {
            foreach (var instanceObject in layer.InstanceObjects)
            {
                if (locations.ContainsValue(instanceObject.InstanceId))
                {
                    var fateId = locations.First(kv => kv.Value == instanceObject.InstanceId).Key;
                    var transform = instanceObject.Transform;
                    var pos = transform.Translation;
                    FateLocations[fateId] = new Vector3(pos.X, pos.Y, pos.Z);
                }
            }
        }
    }

    public override string GetName()
    {
        return "Fates";
    }

    public override void Draw(DebugModule module)
    {
        OcelotUI.Title("Fates:");
        OcelotUI.Indent(() =>
        {
            foreach (var data in EventData.Fates.Values)
            {
                ImGui.TextUnformatted(data.Name);

                if (module.TryGetModule<TeleporterModule>(out var teleporter) && teleporter!.IsReady())
                {
                    var start = FateLocations[data.id];

                    teleporter.teleporter.Button(data.aethernet, start, data.Name, $"fate_{data.id}", data);
                }

                OcelotUI.Indent(() => EventIconRenderer.Drops(data, module.plugin.config.EventDropConfig));

                if (data.id != EventData.Fates.Keys.Max())
                {
                    OcelotUI.VSpace();
                }
            }
        });
    }

    public override void OnTerritoryChanged(ushort id, DebugModule module)
    {
        ProcessLgbData(id);
    }
}
