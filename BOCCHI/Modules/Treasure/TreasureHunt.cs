using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BOCCHI.Chains;
using BOCCHI.Enums;
using BOCCHI.Modules.StateManager;
using BOCCHI.Pathfinding;
using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.Automation.NeoTaskManager;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using ImGuiNET;
using Ocelot;
using Ocelot.Chain;
using Ocelot.IPC;

namespace BOCCHI.Modules.Treasure;

using TreasureData = (uint id, Vector3 position, uint type);

public class TreasureHunt
{
    private const float INTERACT_THRESHOLD = 2f;

    private readonly Dictionary<uint, uint> Levels = new()
    {
        { 1789, 5 },
        { 1790, 11 },
        { 1791, 25 },
        { 1792, 16 },
        { 1793, 14 },
        { 1794, 23 },
        { 1795, 25 },
        { 1796, 28 },
        { 1797, 1 },
        { 1798, 1 },
        { 1799, 2 },
        { 1800, 2 },
        { 1801, 3 },
        { 1802, 3 },
        { 1803, 4 },
        { 1804, 4 },
        { 1805, 5 },
        { 1806, 5 },
        { 1807, 3 },
        { 1808, 6 },
        { 1809, 6 },
        { 1810, 7 },
        { 1811, 8 },
        { 1812, 8 },
        { 1813, 9 },
        { 1814, 9 },
        { 1815, 10 },
        { 1816, 10 },
        { 1817, 11 },
        { 1818, 11 },
        { 1819, 12 },
        { 1820, 12 },
        { 1821, 13 },
        { 1822, 13 },
        { 1823, 14 },
        { 1824, 14 },
        { 1825, 15 },
        { 1826, 15 },
        { 1827, 16 },
        { 1828, 16 },
        { 1829, 17 },
        { 1830, 17 },
        { 1831, 18 },
        { 1832, 18 },
        { 1833, 19 },
        { 1834, 19 },
        { 1835, 20 },
        { 1836, 20 },
        { 1837, 21 },
        { 1838, 21 },
        { 1839, 22 },
        { 1840, 22 },
        { 1841, 22 },
        { 1842, 23 },
        { 1843, 24 },
        { 1844, 24 },
        { 1845, 25 },
        { 1846, 25 },
        { 1847, 26 },
        { 1848, 26 },
        { 1849, 27 },
        { 1850, 27 },
        { 1851, 28 },
        { 1852, 28 },
        { 1853, 21 },
        { 1854, 10 },
        { 1855, 11 },
        { 1856, 11 },
    };

    private List<TreasureData> Treasure = [];

    private bool running = false;

    private Pathfinder? pathfinder;

    private List<PathfinderStep> Steps = [];

    private int stepIndex = 0;

    private float distance = 0f;

    private Stopwatch stopwatch = new();

    private PathfinderStep CurrentStep
    {
        get => Steps[stepIndex];
    }

    public static ChainQueue StepProcessor
    {
        get => ChainManager.Get("TreasureStepProcessor");
    }

    private Dictionary<PathfinderStepType, Func<TreasureModule, VNavmesh, Lifestream, bool>> Handlers = new();

    public unsafe TreasureHunt()
    {
        stopwatch.Reset();

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


        Handlers = new Dictionary<PathfinderStepType, Func<TreasureModule, VNavmesh, Lifestream, bool>>
        {
            {
                PathfinderStepType.WalkToDestination, (module, vnav, lifestream) =>
                {
                    var destination = Treasure.First(t => t.id == CurrentStep.NodeId).position;

                    if (!vnav.IsRunning())
                    {
                        vnav.PathfindAndMoveTo(destination, false);
                    }

                    if (!Player.Mounted)
                    {
                        StepProcessor.SubmitFront(ChainHelper.MountChain());
                    }

                    distance = Player.DistanceTo(destination);
                    if (distance <= module.config.ChestDetectionRange)
                    {
                        var treasure = Svc.Objects
                            .Where(o => o.ObjectKind == ObjectKind.Treasure && o.IsValid() && o is { IsDead: false, IsTargetable: true })
                            .FirstOrDefault(t => Is(t.Position, destination));

                        if (treasure == null)
                        {
                            vnav.Stop();
                            return true;
                        }

                        if (distance <= INTERACT_THRESHOLD)
                        {
                            StepProcessor.SubmitFront(() => Chain.Create().Then(NeoTasks.InteractWithObject(() => treasure)));
                        }
                    }

                    return distance <= INTERACT_THRESHOLD;
                }
            },
            {
                PathfinderStepType.WalkToAethernet, (module, vnav, lifestream) =>
                {
                    var destination = CurrentStep.Aethernet.GetData().position;

                    if (!vnav.IsRunning())
                    {
                        vnav.PathfindAndMoveTo(destination, false);
                    }

                    if (!Player.Mounted)
                    {
                        StepProcessor.SubmitFront(ChainHelper.MountChain());
                    }

                    distance = Player.DistanceTo(destination);
                    return distance <= 4f;
                }
            },
            {
                PathfinderStepType.ReturnToBaseCamp, (module, vnav, lifestream) =>
                {
                    distance = 0;
                    var state = module.GetModule<StateManagerModule>();
                    var inCombat = state.GetState() == State.InCombat;

                    // If we are in combat, start running back to the base camp so we can escape combat
                    if (inCombat && !vnav.IsRunning())
                    {
                        vnav.PathfindAndMoveTo(Aethernet.BaseCamp.GetData().position, false);
                        return false;
                    }

                    if (!inCombat && vnav.IsRunning())
                    {
                        vnav.Stop();
                    }

                    if (inCombat)
                    {
                        return false;
                    }

                    StepProcessor.SubmitFront(ChainHelper.ReturnChain());

                    return true;
                }
            },
            {
                PathfinderStepType.TeleportToAethernet, (module, vnav, lifestream) =>
                {
                    StepProcessor.SubmitFront(ChainHelper.TeleportChain(CurrentStep.Aethernet));
                    return true;
                }
            },
        };
    }

    public void Tick(TreasureModule module)
    {
        if (!running || Plugin.Chain.IsRunning)
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

        if (pathfinder == null && Steps.Count <= 0)
        {
            Svc.Log.Info("Creating new pathfinder");
            pathfinder = new Pathfinder(module, Treasure);
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

        if (pathfinder != null && pathfinder.State != PathfinderState.PathfindingDone)
        {
            Plugin.Chain.Submit(() =>
            {
                Task<List<PathfinderStep>> steps = null;
                var valid = Levels.Where(node => node.Value <= module.config.MaxLevel).Select(node => node.Key).ToList();

                // Prep pathfinding
                return Chain.Create()
                    .Then(new TaskManagerTask(() => pathfinder?.State == PathfinderState.FileLoaded))
                    .Then(_ => steps = pathfinder.FindPath(Player.Position, valid))
                    .Then(new TaskManagerTask(() => steps!.IsCompleted))
                    .Then(_ => Steps = steps!.Result)
                    .Then(_ => pathfinder = null);
            });

            return;
        }

        if (StepProcessor.IsRunning)
        {
            return;
        }

        StepProcessor.Submit(() =>
            Chain.Create()
                .Then(_ =>
                {
                    var handler = Handlers[CurrentStep.Type];
                    if (handler(module, vnav, lifestream))
                    {
                        stepIndex++;
                    }
                })
                .Wait(1000 / 60)
        );


        if (stepIndex < Steps.Count)
        {
            return;
        }

        stopwatch.Stop();
        running = false;
        stepIndex = 0;
        Steps.Clear();
        vnav.Stop();
        Plugin.Chain.Abort();
        StepProcessor.Abort();
        pathfinder = null;
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
            if (ImGui.Button(running ? I18N.T("generic.label.stop") : I18N.T("generic.label.start")))
            {
                running = !running;
                if (running == false)
                {
                    stopwatch.Stop();
                    running = false;
                    stepIndex = 0;
                    Steps.Clear();
                    vnav.Stop();
                    Plugin.Chain.Abort();
                    StepProcessor.Abort();
                    pathfinder = null;
                }
                else
                {
                    stopwatch.Restart();
                }
            }

            if (stopwatch.Elapsed > TimeSpan.Zero)
            {
                OcelotUI.LabelledValue(module.T("panel.hunt.elapsed"), $"{stopwatch.Elapsed:mm\\:ss}");
            }

            if (running)
            {
                OcelotUI.LabelledValue(module.T("panel.hunt.instance.progress"), $"{stepIndex}/{Steps.Count}");

                if (CurrentStep.Type == PathfinderStepType.WalkToDestination)
                {
                    OcelotUI.LabelledValue(module.T("panel.hunt.distance_chest"), $"{distance:f2}/{module.config.ChestDetectionRange:f2}");
                }

                if (CurrentStep.Type == PathfinderStepType.WalkToAethernet)
                {
                    OcelotUI.LabelledValue(module.T("panel.hunt.distance_shard"), $"{distance:f2}");
                }
            }
        });
    }
}
