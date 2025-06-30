using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using Ocelot;
using Ocelot.IPC;

namespace BOCCHI.Modules.Debug.Panels;

using TreasureData = (uint id, Vector3 position, uint type);

public class DistanceEntry
{
    public uint Id { get; set; }

    public float Distance { get; set; }
}

public class TreasureHuntPanel : Panel
{
    private List<TreasureData> Treasure = [];

    private bool FirstTick = false;

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
    }

    public override string GetName()
    {
        return "Treasure Hunt Helper";
    }

    public override void Tick(DebugModule module)
    {
        if (!FirstTick)
        {
            return;
        }

        FirstTick = false;

        _ = CalculatePathDistances(module, Player.Position);
    }


    private async Task CalculatePathDistances(DebugModule module, Vector3 start)
    {
        var stopwatch = Stopwatch.StartNew();
        var vnav = module.GetIPCProvider<VNavmesh>();

        var distancesPath = Path.Join(Svc.PluginInterface.ConfigDirectory.FullName, "distances.json");

        var Distances = await LoadDistancesAsync(distancesPath);
        Dictionary<uint, float> DistanceFromStart = [];

        // var closest = Treasure.OrderBy(t => Vector3.Distance(t.position, start)).Take(10).ToList();  

        foreach (var treasure in Treasure)
        {
            module.Debug($"Treasure {treasure.id}");
            var pathFromStart = await vnav.Pathfind(start, treasure.position, false);
            var distanceFromStart = CalculatePathLength(pathFromStart);
            module.Info($"Path from {treasure.id} to start: {distanceFromStart:0.00}");
            DistanceFromStart[treasure.id] = distanceFromStart;
        }

        var bestPath = FindGreedyPathFromStart(Distances, DistanceFromStart);
        var cost = CalculateTotalPathDistance(bestPath, Distances, DistanceFromStart);

        module.Info($"Path length: {cost:0.00}");

        module.Info($"Path {string.Join(" -> ", bestPath)}");
        module.Info($"Path ({bestPath.Count})");

        stopwatch.Stop();
        module.Info($"Calculating path finished in {stopwatch.ElapsedMilliseconds} ms");
    }


    public override void Draw(DebugModule module)
    {
        OcelotUI.LabelledValue("Bronze", Treasure.Count(t => t.type == 1596).ToString()); // 60
        OcelotUI.LabelledValue("Silver", Treasure.Count(t => t.type == 1597).ToString()); // 8

        OcelotUI.Indent(() =>
        {
            foreach (var data in Treasure)
            {
                OcelotUI.LabelledValue("Id", data.id.ToString());

                OcelotUI.Indent(() =>
                {
                    OcelotUI.LabelledValue("Position", $"{data.position.X:f2}, {data.position.Y:f2}, {data.position.Z:f2}");
                    OcelotUI.LabelledValue("Type", data.type.ToString());
                });
            }
        });
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

    public List<uint> FindGreedyPathFromStart(
        Dictionary<uint, List<DistanceEntry>> closest,
        Dictionary<uint, float> distanceFromStart)
    {
        var unvisited = new HashSet<uint>(distanceFromStart.Keys);
        var path = new List<uint>();

        // Step 1: Find closest node from start
        uint? current = null;
        var minStartDistance = float.MaxValue;

        foreach (var kvp in distanceFromStart)
        {
            if (kvp.Value < minStartDistance)
            {
                minStartDistance = kvp.Value;
                current = kvp.Key;
            }
        }

        if (current == null)
        {
            return path;
        }

        path.Add(current.Value);
        unvisited.Remove(current.Value);

        while (unvisited.Count > 0)
        {
            var currentId = current.Value;

            if (!closest.TryGetValue(currentId, out var neighbors))
            {
                break;
            }

            var bestDistance = float.MaxValue;
            uint? next = null;

            foreach (var entry in neighbors)
            {
                if (unvisited.Contains(entry.Id) && entry.Distance < bestDistance)
                {
                    bestDistance = entry.Distance;
                    next = entry.Id;
                }
            }

            if (next == null)
            {
                break;
            }

            current = next;
            path.Add(current.Value);
            unvisited.Remove(current.Value);
        }

        return path;
    }

    public float CalculateTotalPathDistance(
        List<uint> path,
        Dictionary<uint, List<DistanceEntry>> closest,
        Dictionary<uint, float> distanceFromStart)
    {
        if (path.Count == 0)
        {
            return 0f;
        }

        var total = distanceFromStart[path[0]];

        for (var i = 1; i < path.Count; i++)
        {
            var from = path[i - 1];
            var to = path[i];

            var neighbors = closest[from];
            var match = neighbors.FirstOrDefault(n => n.Id == to);

            total += match.Distance;
        }

        return total;
    }

    public async Task<Dictionary<uint, List<DistanceEntry>>> LoadDistancesAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path);

        var data = JsonSerializer.Deserialize<Dictionary<uint, List<DistanceEntry>>>(json);

        return data!;
    }
}
