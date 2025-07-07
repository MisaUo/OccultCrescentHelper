using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.Carrots;
using BOCCHI.Modules.Treasure;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using ImGuiNET;
using Ocelot;
using Ocelot.IPC;

namespace BOCCHI.Modules.Debug.Panels;

public class CarrotHuntPanel : Panel
{
    private bool HasRun = false;

    private bool ShouldRun = false;

    private Stopwatch stopwatch = new();

    private Task? task = null;

    private uint Progress = 0;

    private readonly uint MaxProgress = 0;

    public unsafe CarrotHuntPanel()
    {
        MaxProgress = (uint)(CarrotData.Data.Count * (CarrotData.Data.Count - 1));
        MaxProgress += (uint)(Enum.GetNames(typeof(Aethernet)).Length * CarrotData.Data.Count);
    }

    public override string GetName()
    {
        return "Carrot Hunt Helper";
    }

    public override void Draw(DebugModule module)
    {
        OcelotUI.LabelledValue("Carrots", CarrotData.Data.Count); // 25

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

        task = PrecomputeCarrotPathDistances(module);
    }

    private async Task PrecomputeCarrotPathDistances(DebugModule module)
    {
        stopwatch.Restart();
        var outputFile = Path.Join(Svc.PluginInterface.ConfigDirectory.FullName, "southhorn_precomputed_carrot_paths.json");

        var vnav = module.GetIPCProvider<VNavmesh>();

        CarrotDataSchema data = new();
        foreach (var datum in AethernetData.All())
        {
            data.AethernetToCarrotDistances[datum.aethernet] = [];
        }

        var index = 0;
        foreach (var carrot in CarrotData.Data)
        {
            data.CarrotToCarrotDistances[carrot.Id] = [];
            data.CarrotsToAethernetDistances[carrot.Id] = [];

            foreach (var other in CarrotData.Data.Where(c => c.Id != carrot.Id))
            {
                var path = await vnav.Pathfind(carrot.Position, other.Position, false);
                var distance = CalculatePathLength(path);

                data.CarrotToCarrotDistances[carrot.Id].Add(new ToCarrot(other.Id, distance));

                Progress++;
            }

            foreach (var datum in AethernetData.All())
            {
                var pathToTreasure = await vnav.Pathfind(datum.position, carrot.Position, false);
                data.AethernetToCarrotDistances[datum.aethernet].Add(new ToCarrot(carrot.Id, CalculatePathLength(pathToTreasure)));

                var pathToAethernet = await vnav.Pathfind(datum.position, carrot.Position, false);
                data.CarrotsToAethernetDistances[carrot.Id].Add(new ToAethernet(datum.aethernet, CalculatePathLength(pathToAethernet)));
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
