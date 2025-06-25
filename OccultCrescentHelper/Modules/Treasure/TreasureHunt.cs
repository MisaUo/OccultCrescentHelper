using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.NeoTaskManager;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ImGuiNET;
using Ocelot;
using Ocelot.Chain;
using Ocelot.IPC;

namespace BOCCHI.Modules.Treasure;

public struct TreasureNode
{
    public Vector3 position;

    public int level;

    public TreasureNode(Vector3 position, int level)
    {
        this.position = position;
        this.level = level;
    }
}

public class TreasureHunt
{
    private const float INTERACT_THRESHOLD = 2f;

    private readonly List<TreasureNode> loop =
    [
        new(new Vector3(490.41f, 62.48f, -590.57f), 1),
        new(new Vector3(617.09f, 66.31f, -703.88f), 1),
        new(new Vector3(666.54f, 79.13f, -480.36f), 2),
        new(new Vector3(870.69f, 95.7f, -388.33f), 2),
        new(new Vector3(779.02f, 96.1f, -256.24f), 4),
        new(new Vector3(770.75f, 108f, -143.54f), 5),
        new(new Vector3(726.28f, 108.15f, -67.9f), 5),
        new(new Vector3(788.88f, 120.4f, 109.39f), 20),
        new(new Vector3(609.62f, 108f, 117.29f), 5),
        new(new Vector3(475.73f, 96f, -87.08f), 4),
        new(new Vector3(354.12f, 95.66f, -288.9f), 3),
        new(new Vector3(245.62f, 109.14f, -18.17f), 9),
        new(new Vector3(277.81f, 103.8f, 241.91f), 10),
        new(new Vector3(517.75f, 67.9f, 236.13f), 21),
        new(new Vector3(643f, 70f, 407.8f), 16),
        new(new Vector3(697.32f, 70f, 597.92f), 17),
        new(new Vector3(596.49f, 70.3f, 622.77f), 17),
        new(new Vector3(471.21f, 70.3f, 530.02f), 16),
        new(new Vector3(433.71f, 70.3f, 683.53f), 17),
        new(new Vector3(294.91f, 56.1f, 640.22f), 15),
        new(new Vector3(256.15f, 73.19f, 492.36f), 14),
        new(new Vector3(140.98f, 56f, 770.99f), 15),
        new(new Vector3(35.72f, 65.11f, 648.98f), 14),
        new(new Vector3(-197.19f, 74.93f, 618.34f), 18),
        new(new Vector3(-225.02f, 75f, 804.99f), 18),
        new(new Vector3(-372.67f, 75f, 527.43f), 19),
        new(new Vector3(-550.12f, 107f, 627.77f), 26),
        new(new Vector3(-784.75f, 139f, 699.78f), 27),
        new(new Vector3(-600.27f, 139f, 802.64f), 28),
        new(new Vector3(-676.39f, 171f, 640.41f), 28),
        new(new Vector3(-716.12f, 171f, 794.43f), 28),
        new(new Vector3(-645.66f, 203f, 710.17f), 28),
        new(new Vector3(-648f, 75f, 403.98f), 19),
        new(new Vector3(-283.96f, 116f, 377.04f), 11),
        new(new Vector3(-256.86f, 121f, 125.08f), 11),
        new(new Vector3(-25.68f, 102.23f, 150.19f), 10),
        new(new Vector3(55.31f, 111.32f, -289.08f), 9),
        new(new Vector3(-158.65f, 98.65f, -132.74f), 11),
        new(new Vector3(-487.11f, 98.53f, -205.46f), 11),
        new(new Vector3(-444.11f, 90.69f, 26.23f), 12),
        new(new Vector3(-394.89f, 106.74f, 175.46f), 12),
        new(new Vector3(-682.77f, 135.62f, -195.27f), 13),
        new(new Vector3(-713.8f, 62.07f, 192.64f), 13),
        new(new Vector3(-756.8f, 76.57f, 97.37f), 13),
        new(new Vector3(-729.92f, 116.54f, -79.06f), 24),
        new(new Vector3(-856.93f, 68.85f, -93.14f), 24),
        new(new Vector3(-767.45f, 115.62f, -235f), 25),
        new(new Vector3(-680.54f, 104.86f, -354.79f), 25),
        new(new Vector3(-798.21f, 105.61f, -310.54f), 25),
        new(new Vector3(-140.46f, 22.38f, -414.27f), 6),
        new(new Vector3(-490.99f, 3f, -529.59f), 8),
        new(new Vector3(-661.68f, 3f, -579.49f), 22),
        new(new Vector3(-729.43f, 5f, -724.79f), 22),
        new(new Vector3(-825.14f, 3f, -832.25f), 23),
        new(new Vector3(-585.26f, 5f, -864.84f), 22),
        new(new Vector3(-451.68f, 3f, -775.57f), 8),
        new(new Vector3(-118.97f, 5f, -708.43f), 7),
        new(new Vector3(142.11f, 16.41f, -574.06f), 6),
        new(new Vector3(381.77f, 22.18f, -743.65f), 6),
        new(new Vector3(386.95f, 96.82f, -451.35f), 3),
    ];

    private volatile float distance;

    private int nodeIndex;

    private List<Vector3> path = [];

    private bool running;

    private volatile IGameObject? treasure;

    private Vector3 currentNode
    {
        get => path[nodeIndex];
    }

    private bool isFinalNode
    {
        get => nodeIndex >= path.Count - 1;
    }

    public void Tick(TreasureModule module)
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

        if (path.Count == 0)
        {
            path = GeneratePath(Player.Position, module);
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

        Plugin.Chain.Submit(() =>
        {
            var pos = $"({currentNode.X:f2}, {currentNode.Y:f2}, {currentNode.Z:f2})";
            return Chain.Create($"Treasure hunt looper ({nodeIndex + 1}/{path.Count}) ({pos})")
                .Then(new TaskManagerTask(() =>
                {
                    if (!vnav.IsRunning())
                    {
                        vnav.PathfindAndMoveTo(currentNode, false);
                    }

                    var treasures = Svc.Objects
                        .Where(o => o != null && o?.ObjectKind == ObjectKind.Treasure &&
                                    o.IsValid() && !o.IsDead && o.IsTargetable)
                        .ToList();

                    var distance = Vector3.Distance(Player.Position, currentNode);
                    this.distance = distance;
                    if (distance <= module.config.ChestDetectionRange)
                    {
                        treasure = treasures.FirstOrDefault(o => Is(o.Position, currentNode));
                        if (treasure != null)
                        {
                            Svc.Targets.Target = treasure;

                            if (distance > INTERACT_THRESHOLD)
                            {
                                return false;
                            }

                            Plugin.Chain.SubmitFront(() => Chain.Create("Interact")
                                .Then(_ => module.Debug("Starting Interaction"))
                                .Then(NeoTasks.InteractWithObject(
                                    () => treasure, false,
                                    new TaskManagerConfiguration
                                    {
                                        TimeLimitMS = 3000,
                                        TimeoutSilently = true,
                                    }))
                                .Then(_ => module.Debug("Interaction Done"))
                            );

                            vnav.Stop();
                        }

                        if (isFinalNode)
                        {
                            running = false;
                            return true;
                        }

                        nodeIndex++;
                        vnav.PathfindAndMoveTo(currentNode, false);
                        return true;
                    }

                    return false;
                }, new TaskManagerConfiguration { TimeLimitMS = int.MaxValue }))
                .Then(() => Chain.Create("Interact")
                    .Wait(300)
                    .Then(NeoTasks.InteractWithObject(
                        () => treasure, false,
                        new TaskManagerConfiguration { TimeLimitMS = 3000 }))
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

        OcelotUI.Title($"{module.T("panel.hunt.title")}:");
        OcelotUI.Indent(() =>
        {
            if (ImGui.Button(running ? "Stop" : "Start"))
            {
                path.Clear();
                nodeIndex = 0;
                running = !running;
                distance = 0f;
                if (!running)
                {
                    vnav.Stop();
                    Plugin.Chain.Abort();
                }
            }

            if (running)
            {
                OcelotUI.LabelledValue(module.T("panel.hunt.distance"), distance.ToString());

                var instances = ChainManager.Active();

                OcelotUI.LabelledValue(module.T("panel.hunt.count"), instances.Count.ToString());

                foreach (var pair in instances)
                {
                    if (pair.Value.CurrentChain == null)
                    {
                        continue;
                    }

                    OcelotUI.Title($"{pair.Key}:");
                    OcelotUI.Indent(() =>
                    {
                        var current = pair.Value.CurrentChain!;
                        OcelotUI.LabelledValue(module.T("panel.hunt.instance.current"), distance.ToString());
                        OcelotUI.LabelledValue(module.T("panel.hunt.instance.progress"), $"{current.progress * 100}%");
                        OcelotUI.LabelledValue(module.T("panel.hunt.instance.queue"), pair.Value.QueueCount.ToString());
                    });
                }
            }
        });
    }

    private List<Vector3> GeneratePath(Vector3 from, TreasureModule module)
    {
        List<Vector3> path = new();

        var valid = loop.Where(node => node.level <= module.config.MaxLevel).ToList();
        if (valid.Count <= 0)
        {
            return path;
        }

        var startIndex = 0;
        var closestDistance = float.MaxValue;

        for (var i = 0; i < valid.Count; i++)
        {
            var dist = Vector3.Distance(from, valid[i].position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                startIndex = i;
            }
        }

        for (var i = 0; i < valid.Count; i++)
        {
            var index = (startIndex + i) % valid.Count;
            path.Add(valid[index].position);
        }

        var start = path.First();
        module.Debug(
            $"Generated path with {path.Count} nodes, starting at ({startIndex}) {start.X:f2}, {start.Y:f2}, {start.Z:f2}");

        return path;
    }
}
