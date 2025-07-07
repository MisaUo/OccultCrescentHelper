using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using BOCCHI.Enums;
using BOCCHI.Modules.Treasure;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using ImGuiNET;
using Ocelot;
using Ocelot.IPC;

namespace BOCCHI.Modules.Debug.Panels;

using TreasureData = (uint id, Vector3 position, uint type);

public class TreasureHuntPanel : Panel
{
    private List<TreasureData> Treasure = [];

    private bool HasRun = false;

    private bool ShouldRun = false;

    private Stopwatch stopwatch = new();

    private Task? task = null;

    private uint Progress = 0;

    private readonly uint MaxProgress = 0;

    public unsafe TreasureHuntPanel()
    {
        var layout = LayoutWorld.Instance()->ActiveLayout;
        if (layout == null)
        {
            return;
        }

        if (!layout->InstancesByType.TryGetValue(InstanceType.Treasure, out var mapPtr, false))
        {
            return;
        }

        foreach (ILayoutInstance* instance in mapPtr.Value->Values)
        {
            var transform = instance->GetTransformImpl();
            var position = transform->Translation;
            if (position.Y <= -10f)
            {
                continue;
            }

            var treasureRowId = Unsafe.Read<uint>((byte*)instance + 0x30);
            var sgbId = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Treasure>().GetRow(treasureRowId).SGB.RowId;
            if (sgbId != 1596 && sgbId != 1597)
            {
                continue;
            }

            Treasure.Add((treasureRowId, position, sgbId));
        }

        Treasure = Treasure.OrderBy(t => t.id).ToList();

        MaxProgress = (uint)(Treasure.Count * (Treasure.Count - 1));
        MaxProgress += (uint)(Enum.GetNames(typeof(Aethernet)).Length * Treasure.Count) * 2;
    }

    public override string GetName()
    {
        return "Treasure Hunt Helper";
    }

    public override void Draw(DebugModule module)
    {
        OcelotUI.LabelledValue("Bronze", Treasure.Count(t => t.type == 1596)); // 60
        OcelotUI.LabelledValue("Silver", Treasure.Count(t => t.type == 1597)); // 8

        OcelotUI.Indent(() =>
        {
            if (!HasRun)
            {
                if (ImGui.Button("Run"))
                {
                    ShouldRun = true;
                }

                return;
            }

            var Completion = (float)Progress / (float)MaxProgress * 100;

            OcelotUI.LabelledValue("Progress: ", $"{Completion:f2}%");
            OcelotUI.Indent(() => OcelotUI.LabelledValue("Calculations: ", $"{Progress}/{MaxProgress}"));
            OcelotUI.LabelledValue("Elapsed: ", stopwatch.Elapsed.ToString("mm\\:ss"));
        });
    }

    public override void Tick(DebugModule module)
    {
        if (!ShouldRun || HasRun || task != null)
        {
            return;
        }

        ShouldRun = true;
        HasRun = true;

        task = PrecomputeTreasurePathDistances(module);
    }

    private async Task PrecomputeTreasurePathDistances(DebugModule module)
    {
        stopwatch.Restart();
        var outputFile = Path.Join(Svc.PluginInterface.ConfigDirectory.FullName, "southhorn_precomputed_chest_paths.json");

        var vnav = module.GetIPCProvider<VNavmesh>();

        TreasureDataSchema data = new();
        foreach (var datum in AethernetData.All())
        {
            data.AethernetToTreasureDistances[datum.aethernet] = [];
        }

        foreach (var treasure in Treasure)
        {
            data.TreasureToTreasureDistances[treasure.id] = [];
            data.TreasureToAethernetDistances[treasure.id] = [];

            foreach (var other in Treasure.Where(t => t != treasure))
            {
                var path = await vnav.Pathfind(treasure.position, other.position, false);
                var distance = CalculatePathLength(path);

                data.TreasureToTreasureDistances[treasure.id].Add(new ToTreasure(other.id, distance));

                Progress++;
            }

            foreach (var datum in AethernetData.All())
            {
                var pathToTreasure = await vnav.Pathfind(datum.position, treasure.position, false);
                data.AethernetToTreasureDistances[datum.aethernet].Add(new ToTreasure(treasure.id, CalculatePathLength(pathToTreasure)));

                var pathToAethernet = await vnav.Pathfind(datum.position, treasure.position, false);
                data.TreasureToAethernetDistances[treasure.id].Add(new ToAethernet(datum.aethernet, CalculatePathLength(pathToAethernet)));
            }
        }

        stopwatch.Stop();

        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            IncludeFields = false,
        };

        var json = JsonSerializer.Serialize(data, options);
        await File.WriteAllTextAsync(outputFile, json);
    }

    private float CalculatePathLength(List<Vector3> path)
    {
        var length = 0f;

        for (var i = 1; i < path.Count; i++)
        {
            length += Vector3.Distance(path[i - 1], path[i]);
        }

        return length;
    }
}
