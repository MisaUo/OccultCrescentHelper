
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.NeoTaskManager;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ImGuiNET;
using OccultCrescentHelper.Chains;
using Ocelot;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;

namespace OccultCrescentHelper.Modules.Treasure;

public class TreasureHunt
{
    private const float INTERACT_THRESHOLD = 2f;

    private const float DATA_THRESHOLD = 75f;

    private List<List<Vector3>> paths = [
        [
            new Vector3(617.09f, 66.31f, -703.88f),
            // new Vector3(490.41f, 62.48f, -590.57f),
            // new Vector3(666.54f, 79.13f, -480.36f),
            // new Vector3(870.69f, 95.7f, -388.33f),
            // new Vector3(779.02f, 96.1f, -256.24f),
            // new Vector3(770.75f, 108f, -143.54f),
            // new Vector3(726.28f, 108.15f, -67.9f),
            // new Vector3(788.88f, 120.4f, 109.39f),
            // new Vector3(609.62f, 108f, 117.29f),
            // new Vector3(475.73f, 96f, -87.08f),
            // new Vector3(245.62f, 109.14f, -18.17f),
            // new Vector3(-25.68f, 102.23f, 150.19f),
            // new Vector3(-158.65f, 98.65f, -132.74f),
            // new Vector3(55.31f, 111.32f, -289.08f),
            // new Vector3(354.12f, 95.66f, -288.9f),
            // new Vector3(386.95f, 96.82f, -451.35f),
            // new Vector3(381.77f, 22.18f, -743.65f),
            // new Vector3(142.11f, 16.41f, -574.06f),
            // new Vector3(-118.97f, 5f, -708.43f),
            // new Vector3(-140.46f, 22.38f, -414.27f),
            // new Vector3(-343.16f, 52.35f, -382.13f),
            // new Vector3(-490.99f, 3f, -529.59f),
            // new Vector3(-451.68f, 3f, -775.57f),
            // new Vector3(-585.26f, 5f, -864.84f),
            // new Vector3(-729.43f, 5f, -724.79f),
            // new Vector3(-825.14f, 3f, -832.25f),
            // new Vector3(-661.68f, 3f, -579.49f),
        ],
        [
            new Vector3(277.81f, 103.8f, 241.91f),
            // new Vector3(517.75f, 67.9f, 236.13f),
            // new Vector3(643f, 70f, 407.8f),
            // new Vector3(697.32f, 70f, 597.92f),
            // new Vector3(835.11f, 70f, 699.12f),
            // new Vector3(596.49f, 70.3f, 622.77f),
            // new Vector3(471.21f, 70.3f, 530.02f),
            // new Vector3(433.71f, 70.3f, 683.53f),
            // new Vector3(294.91f, 56.1f, 640.22f),
            // new Vector3(256.15f, 73.19f, 492.36f),
            // new Vector3(35.72f, 65.11f, 648.98f),
            // new Vector3(140.98f, 56f, 770.99f),
            // new Vector3(-225.02f, 75f, 804.99f),
            // new Vector3(-197.19f, 74.93f, 618.34f),
            // new Vector3(-372.67f, 75f, 527.43f),
            // new Vector3(-648f, 75f, 403.98f),
            // new Vector3(-401.63f, 85.06f, 332.54f),
            // new Vector3(-283.96f, 116f, 377.04f),
            // new Vector3(-256.86f, 121f, 125.08f),
        ],
        [
            new Vector3(-487.11f, 98.53f, -205.46f),
            new Vector3(-444.11f, 90.69f, 26.23f),
            new Vector3(-394.89f, 106.74f, 175.46f),
            new Vector3(-682.77f, 135.62f, -195.27f),
            new Vector3(-713.8f, 62.07f, 192.64f),
            new Vector3(-756.8f, 76.57f, 97.37f),
            new Vector3(-729.92f, 116.54f, -79.06f),
            new Vector3(-856.93f, 68.85f, -93.14f),
            new Vector3(-767.45f, 115.62f, -235f),
            new Vector3(-680.54f, 104.86f, -354.79f),
            new Vector3(-798.21f, 105.61f, -310.54f),
            new Vector3(-487.11f, 98.53f, -205.46f),
        ]
    ];

    private int pathIndex = 0;

    private List<Vector3> currentPath => paths[pathIndex];

    private bool isFinalPath => pathIndex >= paths.Count - 1;

    private int nodeIndex = 0;

    private Vector3 currentNode => currentPath[nodeIndex];

    private bool isFinalNode => nodeIndex >= currentPath.Count - 1;

    private bool running = false;

    private volatile float distance = 0f;

    private volatile IGameObject? treasure = null;

    public unsafe void Tick(TreasureModule module)
    {
        if (!running)
        {
            return;
        }

        if (!module.TryGetIPCProvider<VNavmesh>(out var vnav) || vnav == null)
        {
            return;
        }

        if (!module.TryGetIPCProvider<Lifestream>(out var lifestream) || lifestream == null)
        {
            return;
        }

        MaintainWatcherChain(module, vnav, lifestream);
    }

    private bool Is(Vector3 a, Vector3 b, float variance = 5f)
    {
        return Vector3.Distance(a, b) <= variance;
    }

    private void MaintainWatcherChain(TreasureModule module, VNavmesh vnav, Lifestream lifestream)
    {
        if (Plugin.Chain.IsRunning)
        {
            return;
        }

        Plugin.Chain.Submit(
            () => {
                return Chain.Create($"Treasure hunt looper ({pathIndex + 1}:{nodeIndex + 1})")
                    .Then(new TaskManagerTask(() => {
                        if (!vnav.IsRunning())
                        {
                            vnav.PathfindAndMoveTo(currentNode, false);
                        }

                        var treasures = Svc.Objects
                            .Where(o => o != null && o?.ObjectKind == ObjectKind.Treasure && o.IsValid() && !o.IsDead && o.IsTargetable)
                            .ToList();

                        var distance = Vector3.Distance(Player.Position, currentNode);
                        this.distance = distance;
                        if (distance <= DATA_THRESHOLD)
                        {
                            treasure = treasures.FirstOrDefault(o => Is(o.Position, currentNode));
                            if (treasure != null)
                            {
                                Svc.Targets.Target = treasure;

                                if (distance > INTERACT_THRESHOLD)
                                {
                                    return false;
                                }

                                Svc.Log.Info("Chest!");
                                Plugin.Chain.Submit(
                                    () => Chain.Create("Interact")
                                        .Then(_ => module.Debug("Starting Interaction"))
                                        .Then(NeoTasks.InteractWithObject(() => treasure, false, new() {
                                            TimeLimitMS = 3000
                                        }))
                                        .Then(_ => module.Debug("Interaction Done"))
                                );

                                vnav.Stop();
                            }

                            if (isFinalNode)
                            {
                                if (isFinalPath)
                                {
                                    running = false;
                                    return true;
                                }

                                pathIndex++;
                                nodeIndex = 0;
                                OnPathChanged(module, vnav, lifestream);
                                return true;
                            }

                            nodeIndex++;
                            vnav.PathfindAndMoveTo(currentNode, false);
                            return true;
                        }

                        return false;
                    }, new() { TimeLimitMS = int.MaxValue }))
                    .Then(() => Chain.Create("Interact")
                        .Wait(300)
                        .Then(NeoTasks.InteractWithObject(() => treasure, false, new() { TimeLimitMS = 3000 }))
                        .Then(_ => treasure = null)
                    );
            });
    }

    public void Draw(TreasureModule module)
    {
        if (!module.TryGetIPCProvider<VNavmesh>(out var vnav) || vnav == null)
        {
            return;
        }

        if (!module.TryGetIPCProvider<Lifestream>(out var lifestream) || lifestream == null)
        {
            return;
        }

        OcelotUI.Title("Treasure Hunter:");
        OcelotUI.Indent(() => {
            if (ImGui.Button(running ? "Stop" : "Start"))
            {
                pathIndex = 0;
                nodeIndex = 0;
                running = !running;
                distance = 0f;
                if (running == false)
                {
                    vnav.Stop();
                    Plugin.Chain.Abort();
                }
                else
                {
                    OnPathChanged(module, vnav, lifestream);
                }
            }

            if (running)
            {
                OcelotUI.Title("Distance to next node:");
                ImGui.SameLine();
                ImGui.TextUnformatted(distance.ToString());

                var instances = ChainManager.Active();
                OcelotUI.Title("# of instances:");
                ImGui.SameLine();
                ImGui.TextUnformatted(instances.Count.ToString());

                foreach (var pair in instances)
                {
                    if (pair.Value.CurrentChain == null)
                    {
                        continue;
                    }

                    OcelotUI.Title($"{pair.Key}:");
                    OcelotUI.Indent(() => {
                        var current = pair.Value.CurrentChain!;
                        OcelotUI.Title("Current Chain:");
                        ImGui.SameLine();
                        ImGui.TextUnformatted(current.name);

                        OcelotUI.Title("Progress:");
                        ImGui.SameLine();
                        ImGui.TextUnformatted($"{current.progress * 100}%");

                        OcelotUI.Title("Queued Chains:");
                        ImGui.SameLine();
                        ImGui.TextUnformatted(pair.Value.QueueCount.ToString());
                    });
                }
            }
        });
    }

    private void OnPathChanged(TreasureModule module, VNavmesh vnav, Lifestream lifestream)
    {
        vnav.Stop();
        Plugin.Chain.Abort();
        switch (pathIndex)
        {
            case 0:
                module.Warning("0:start");
                // Plugin.Chain.Submit(
                //     () => Chain.Create("Treasure hunt phase 1 setup")
                //         .Then(new ReturnChain(new Vector3(804.38f, 70.86f, -691.59f), module.GetIPCProvider<YesAlready>(), module.GetIPCProvider<VNavmesh>()))
                //         .Then(new MountChain(module.plugin.config.TeleporterConfig.Mount))
                // );
                break;
            case 1:
                module.Warning("1:start");
                Plugin.Chain.Submit(
                    () => Chain.Create("Treasure hunt phase 2 setup")
                        .ConditionalThen(_ => Svc.Condition[ConditionFlag.InCombat], _ => new TaskManagerTask(() => {
                            if (!vnav.IsRunning())
                            {
                                vnav.PathfindAndMoveTo(new Vector3(-173.02f, 8.19f, -611.14f), false);
                            }

                            return !Svc.Condition[ConditionFlag.InCombat];
                        }, new() { TimeLimitMS = int.MaxValue }))
                        .Then(_ => vnav.Stop())
                        .Then(new ReturnChain(new(830.75f, 72.98f, -695.98f), module.GetIPCProvider<YesAlready>(), module.GetIPCProvider<VNavmesh>()))
                        .WaitForPathfindingCycle(vnav)
                        .Then(new TeleportChain(lifestream, Enums.Aethernet.Eldergrowth))
                        .Then(new MountChain(module.plugin.config.TeleporterConfig.Mount))
                );
                break;
            case 2:
                module.Warning("2:start");
                Plugin.Chain.Submit(
                    () => Chain.Create("Treasure hunt phase 3 setup")
                        .ConditionalThen(_ => Svc.Condition[ConditionFlag.InCombat], _ => new TaskManagerTask(() => {
                            if (!vnav.IsRunning())
                            {
                                vnav.PathfindAndMoveTo(new Vector3(-173.02f, 8.19f, -611.14f), false);
                            }

                            return !Svc.Condition[ConditionFlag.InCombat];
                        }, new() { TimeLimitMS = int.MaxValue }))
                        .Then(_ => vnav.Stop())
                        .Then(new ReturnChain(new(830.75f, 72.98f, -695.98f), module.GetIPCProvider<YesAlready>(), module.GetIPCProvider<VNavmesh>()))
                        .WaitForPathfindingCycle(vnav)
                        .Then(new TeleportChain(lifestream, Enums.Aethernet.CrystallizedCaverns))
                        .Then(new MountChain(module.plugin.config.TeleporterConfig.Mount))
                );
                break;
        }
    }
}
