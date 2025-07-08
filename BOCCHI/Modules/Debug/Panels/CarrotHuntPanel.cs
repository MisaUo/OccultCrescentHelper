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
using BOCCHI.Modules.Data;
using BOCCHI.Modules.Treasure;
using BOCCHI.Pathfinding;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using ImGuiNET;
using Ocelot;
using Ocelot.Chain;
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

    private ChainQueue ChainQueue
    {
        get => ChainManager.Get("CarrotHuntPanelChain");
    }

    public CarrotHuntPanel()
    {
        var carrotCount = CarrotData.Data.Count;
        var aethernetCount = Enum.GetNames(typeof(Aethernet)).Length;

        MaxProgress = (uint)(carrotCount * (carrotCount - 1));
        MaxProgress += (uint)(aethernetCount * carrotCount);
        MaxProgress += (uint)(carrotCount * aethernetCount);
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

    private Task PrecomputeCarrotPathDistances(DebugModule module)
    {
        stopwatch.Restart();
        var outputFile = Path.Join(ZoneData.GetCurrentZoneDataDirectory(), "precomputed_carrot_hunt_data.json");

        var vnav = module.GetIPCProvider<VNavmesh>();

        NodeDataSchema data = new();
        foreach (var datum in AethernetData.All())
        {
            data.AethernetToNodeDistances[datum.aethernet] = [];
        }

        foreach (var carrot in CarrotData.Data)
        {
            data.NodeToNodeDistances[carrot.Id] = [];
            data.NodeToAethernetDistances[carrot.Id] = [];

            foreach (var other in CarrotData.Data.Where(c => c.Id != carrot.Id))
            {
                ChainQueue.Submit(() =>
                    Chain.Create()
                        .Then(async void (_) =>
                        {
                            var path = await vnav.Pathfind(carrot.Position, other.Position, false);
                            var distance = CalculatePathLength(path);

                            var nodes = path.Select(Position.Create).ToList();

                            data.NodeToNodeDistances[carrot.Id].Add(new ToNode(other.Id, distance, nodes));

                            Progress++;
                        })
                        .Then(_ => !vnav.IsRunning())
                );
            }

            foreach (var datum in AethernetData.All())
            {
                ChainQueue.Submit(() =>
                    Chain.Create()
                        .Then(async void (_) =>
                        {
                            var path = await vnav.Pathfind(datum.destination, carrot.Position, false);
                            var distance = CalculatePathLength(path);

                            var nodes = path.Select(Position.Create).ToList();

                            data.AethernetToNodeDistances[datum.aethernet].Add(new ToNode(carrot.Id, distance, nodes));

                            Progress++;
                        })
                        .Then(_ => !vnav.IsRunning())
                );

                ChainQueue.Submit(() =>
                    Chain.Create()
                        .Then(async void (_) =>
                        {
                            var path = await vnav.Pathfind(datum.destination, carrot.Position, false);
                            var distance = CalculatePathLength(path);

                            var nodes = path.Select(Position.Create).ToList();

                            data.NodeToAethernetDistances[carrot.Id].Add(new ToAethernet(datum.aethernet, distance, nodes));

                            Progress++;
                        })
                        .Then(_ => !vnav.IsRunning())
                );
            }
        }

        ChainQueue.Submit(() =>
            Chain.Create()
                .Then(_ =>
                {
                    stopwatch.Stop();

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        IncludeFields = false,
                    };

                    Svc.Log.Info("Saving file to " + outputFile);
                    var json = JsonSerializer.Serialize(data, options);
                    File.WriteAllTextAsync(outputFile, json);
                })
        );
        return Task.CompletedTask;
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
