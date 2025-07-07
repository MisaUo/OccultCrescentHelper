using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using BOCCHI.Enums;
using BOCCHI.Modules.Data;
using BOCCHI.Modules.Treasure;
using BOCCHI.Pathfinding;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using Ocelot.Chain;
using Ocelot.IPC;

namespace BOCCHI.Modules.Carrots;

public class Pathfinder
{
    private List<(uint Id, Vector3 Position, uint Level)> carrots = [];

    public PathfinderState State { get; private set; } = PathfinderState.None;

    private CarrotDataSchema? data;

    private CarrotsModule module;

    private const float RETURN_COST = 300f;

    private const float TELEPORT_COST = 50f;

    public Pathfinder(CarrotsModule module, List<(uint Id, Vector3 Position, uint Level)> carrots)
    {
        this.module = module;
        this.carrots = carrots;

        LoadFile();
    }

    private async void LoadFile()
    {
        State = PathfinderState.LoadingFile;
        var options = new JsonSerializerOptions
        {
            IncludeFields = false,
        };

        // @todo ensure this file exists or crash!
        var file = Path.Join(Svc.PluginInterface.ConfigDirectory.FullName, "southhorn_precomputed_carrot_paths.json");
        var json = await File.ReadAllTextAsync(file);
        data = JsonSerializer.Deserialize<CarrotDataSchema>(json, options);
        State = PathfinderState.FileLoaded;
    }

    public async Task<List<PathfinderStep>> FindPath(Vector3 startPosition, List<uint> nodesToVisit)
    {
        if (State != PathfinderState.FileLoaded)
        {
            throw new Exception("File not loaded");
        }

        if (data == null)
        {
            throw new Exception("Data not loaded");
        }

        State = PathfinderState.Pathfinding;
        var vnav = module.GetIPCProvider<VNavmesh>();

        List<PathfinderStep> steps = [];

        var closestDistance = float.MaxValue;
        var startCarrot = carrots.First();
        foreach (var carrot in carrots)
        {
            if (!nodesToVisit.Contains(carrot.Id))
            {
                continue;
            }

            var distance = Vector3.Distance(startPosition, carrot.Position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                startCarrot = carrot;
            }
        }

        steps.Add(PathfinderStep.WalkToDestination(startCarrot.Id));
        nodesToVisit.Remove(startCarrot.Id);

        while (nodesToVisit.Count > 0)
        {
            var currentId = steps.Last().NodeId;
            var bestCost = float.MaxValue;
            List<PathfinderStep> bestSteps = null;
            uint bestTarget = 0;

            // Option 1: Direct walk to next treasure
            foreach (var toTreasure in data.Value.CarrotToCarrotDistances[currentId])
            {
                if (!nodesToVisit.Contains(toTreasure.Id))
                {
                    continue;
                }

                if (toTreasure.Distance < bestCost)
                {
                    bestCost = toTreasure.Distance;
                    bestSteps = [PathfinderStep.WalkToDestination(toTreasure.Id)];
                    bestTarget = toTreasure.Id;
                }
            }

            var fromAethernet = data.Value.CarrotsToAethernetDistances[currentId]
                .OrderBy(x => x.Distance)
                .First().Aethernet;

            var walkToFromShard = data.Value.CarrotsToAethernetDistances[currentId]
                .First(x => x.Aethernet == fromAethernet).Distance;

            foreach (var (toAethernet, treasureList) in data.Value.AethernetToCarrotDistances)
            {
                if (toAethernet == fromAethernet)
                {
                    continue;
                }

                foreach (var toTreasure in treasureList)
                {
                    if (!nodesToVisit.Contains(toTreasure.Id))
                    {
                        continue;
                    }

                    var cost = walkToFromShard + TELEPORT_COST + toTreasure.Distance;

                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestSteps =
                        [
                            PathfinderStep.WalkToAethernet(fromAethernet),
                            PathfinderStep.TeleportToAethernet(toAethernet),
                            PathfinderStep.WalkToDestination(toTreasure.Id),
                        ];
                        bestTarget = toTreasure.Id;
                    }
                }
            }


            // Option 3: Return to basecamp → teleport → walk
            foreach (var kvp in data.Value.AethernetToCarrotDistances)
            {
                var aethernet = kvp.Key;
                foreach (var toTreasure in kvp.Value)
                {
                    if (!nodesToVisit.Contains(toTreasure.Id))
                    {
                        continue;
                    }

                    var cost = RETURN_COST + TELEPORT_COST + toTreasure.Distance;
                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        if (aethernet == Aethernet.BaseCamp)
                        {
                            bestSteps =
                            [
                                PathfinderStep.ReturnToBaseCamp(),
                                PathfinderStep.WalkToDestination(toTreasure.Id),
                            ];
                        }
                        else
                        {
                            bestSteps =
                            [
                                PathfinderStep.ReturnToBaseCamp(),
                                PathfinderStep.TeleportToAethernet(aethernet),
                                PathfinderStep.WalkToDestination(toTreasure.Id),
                            ];
                        }

                        bestTarget = toTreasure.Id;
                    }
                }
            }

            if (bestSteps != null)
            {
                steps.AddRange(bestSteps);
                nodesToVisit.Remove(bestTarget);
            }
            else
            {
                break;
            }
        }

        PrintPath(steps);

        State = PathfinderState.PathfindingDone;
        return steps;
    }

    private void PrintPath(List<PathfinderStep> steps)
    {
        Svc.Log.Info("== Pathfinder Steps ==");

        var index = 1;
        var treasureCount = 0;

        foreach (var step in steps)
        {
            var message = step.Type switch
            {
                PathfinderStepType.WalkToDestination => $"[{index}] Walk to Carrot ID: {step.NodeId}",
                PathfinderStepType.WalkToAethernet => $"[{index}] Walk to Aethernet: {step.Aethernet}",
                PathfinderStepType.TeleportToAethernet => $"[{index}] Teleport to Aethernet: {step.Aethernet}",
                PathfinderStepType.ReturnToBaseCamp => $"[{index}] Return to Base Camp",
                _ => $"[{index}] Unknown Step Type",
            };

            if (step.Type == PathfinderStepType.WalkToDestination)
            {
                treasureCount++;
            }

            Svc.Log.Info(message);
            index++;
        }

        Svc.Log.Info($"== Total treasures visited: {treasureCount} ==");
        Svc.Log.Info("=======================");
    }
}
