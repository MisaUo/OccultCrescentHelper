// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using System.Numerics;
// using System.Runtime.CompilerServices;
// using System.Text.Json;
// using System.Threading.Tasks;
// using BOCCHI.Chains;
// using BOCCHI.Data;
// using BOCCHI.Enums;
// using BOCCHI.Modules.StateManager;
// using BOCCHI.Pathfinding;
// using Dalamud.Game.ClientState.Objects.Enums;
// using ECommons.Automation.NeoTaskManager;
// using ECommons.Automation.NeoTaskManager.Tasks;
// using ECommons.DalamudServices;
// using ECommons.GameHelpers;
// using ImGuiNET;
// using Ocelot;
// using Ocelot.Chain;
// using Ocelot.IPC;
// using TextCopy;
//
// namespace BOCCHI.Modules.Carrots;
//
// public class CarrotHunt
// {
//     private const float USE_THRESHOLD = 2f;
//
//     private bool running = false;
//
//     private Pathfinder? pathfinder;
//
//     private List<PathfinderStep> Steps = [];
//
//     private int stepIndex = 0;
//
//     private float distance = 0f;
//
//     private Stopwatch stopwatch = new();
//
//     private PathfinderStep CurrentStep
//     {
//         get => Steps[stepIndex];
//     }
//
//     private string JSON = "";
//
//     public ChainQueue StepProcessor
//     {
//         get => ChainManager.Get("CarrotStepProcessor");
//     }
//
//     private Dictionary<PathfinderStepType, Func<CarrotsModule, VNavmesh, Lifestream, bool>> Handlers = new();
//
//     public CarrotHunt()
//     {
//         stopwatch.Reset();
//
//         Handlers = new Dictionary<PathfinderStepType, Func<CarrotsModule, VNavmesh, Lifestream, bool>>
//         {
//             {
//                 PathfinderStepType.WalkToNode, (module, vnav, lifestream) =>
//                 {
//                     var destination = CarrotData.Data.First(c => c.Id == CurrentStep.NodeId).Position;
//
//                     if (!vnav.IsRunning())
//                     {
//                         vnav.PathfindAndMoveTo(destination, false);
//                     }
//
//                     if (!Player.Mounted)
//                     {
//                         StepProcessor.SubmitFront(ChainHelper.MountChain());
//                     }
//
//                     distance = Player.DistanceTo(destination);
//                     if (distance <= module.config.CarrotDetectionRange)
//                     {
//                         var carrot = Svc.Objects
//                             .Where(o => o.ObjectKind == ObjectKind.EventObj && o.DataId == (uint)OccultObjectType.Carrot && o.IsValid() &&
//                                         o is { IsDead: false, IsTargetable: false })
//                             .FirstOrDefault(t => Is(t.Position, destination));
//
//                         if (carrot == null)
//                         {
//                             vnav.Stop();
//                             return true;
//                         }
//
//                         if (distance <= USE_THRESHOLD)
//                         {
//                             Svc.Log.Info("Carrot found!");
//                             return true;
//                             // StepProcessor.SubmitFront(() => Chain.Create().Then(NeoTasks.InteractWithObject(() => carrot)));
//                         }
//                     }
//
//                     return distance <= USE_THRESHOLD;
//                 }
//             },
//             {
//                 PathfinderStepType.WalkToAethernet, (module, vnav, lifestream) =>
//                 {
//                     var destination = CurrentStep.Aethernet.GetData().position;
//
//                     if (!vnav.IsRunning())
//                     {
//                         vnav.PathfindAndMoveTo(destination, false);
//                     }
//
//                     if (!Player.Mounted)
//                     {
//                         StepProcessor.SubmitFront(ChainHelper.MountChain());
//                     }
//
//                     distance = Player.DistanceTo(destination);
//                     return distance <= 4f;
//                 }
//             },
//             {
//                 PathfinderStepType.ReturnToBaseCamp, (module, vnav, lifestream) =>
//                 {
//                     distance = 0;
//                     var state = module.GetModule<StateManagerModule>();
//                     var inCombat = state.GetState() == State.InCombat;
//
//                     // If we are in combat, start running back to the base camp so we can escape combat
//                     if (inCombat && !vnav.IsRunning())
//                     {
//                         vnav.PathfindAndMoveTo(Aethernet.BaseCamp.GetData().position, false);
//                         return false;
//                     }
//
//                     if (!inCombat && vnav.IsRunning())
//                     {
//                         vnav.Stop();
//                     }
//
//                     if (inCombat)
//                     {
//                         return false;
//                     }
//
//                     StepProcessor.SubmitFront(ChainHelper.ReturnChain());
//
//                     return true;
//                 }
//             },
//             {
//                 PathfinderStepType.TeleportToAethernet, (module, vnav, lifestream) =>
//                 {
//                     StepProcessor.SubmitFront(ChainHelper.TeleportChain(CurrentStep.Aethernet));
//                     return true;
//                 }
//             },
//         };
//     }
//
//     public void Tick(CarrotsModule module)
//     {
//         if (!running || Plugin.Chain.IsRunning)
//         {
//             return;
//         }
//
//         if (!module.TryGetIPCProvider<VNavmesh>(out var vnav) || vnav == null)
//         {
//             return;
//         }
//
//         if (!module.TryGetIPCProvider<Lifestream>(out var lifestream) || lifestream == null)
//         {
//             return;
//         }
//
//         if (pathfinder == null && Steps.Count <= 0)
//         {
//             pathfinder = new Pathfinder(module._config.PathfinderConfig.ReturnCost, module._config.PathfinderConfig.TeleportCost);
//         }
//
//         MaintainWatcherChain(module, vnav, lifestream);
//     }
//
//     private bool Is(Vector3 a, Vector3 b, float variance = 5f)
//     {
//         return Vector3.Distance(a, b) <= variance;
//     }
//
//     private void MaintainWatcherChain(CarrotsModule module, VNavmesh vnav, Lifestream lifestream)
//     {
//         if (Plugin.Chain.IsRunning)
//         {
//             return;
//         }
//
//         if (pathfinder != null && pathfinder.State != PathfinderState.PathfindingDone)
//         {
//             Plugin.Chain.Submit(() =>
//             {
//                 Task<List<PathfinderStep>> steps = null;
//                 var valid = CarrotData.Data.Where(node => node.Level <= module.config.MaxLevel).Select(node => node.Id).ToList();
//
//                 // Prep pathfinding
//                 return Chain.Create()
//                     .Then(new TaskManagerTask(() => pathfinder?.State == PathfinderState.FileLoaded))
//                     .Then(_ => steps = pathfinder.FindPath(Player.Position, valid))
//                     .Then(new TaskManagerTask(() => steps!.IsCompleted))
//                     .Then(_ => Steps = steps!.Result)
//                     .Then(_ =>
//                     {
//                         var options = new JsonSerializerOptions
//                         {
//                             WriteIndented = true,
//                             Converters =
//                             {
//                                 new PathfinderStepConverter(),
//                             },
//                         };
//
//                         JSON = JsonSerializer.Serialize(Steps, options);
//                     })
//                     .Then(_ => pathfinder = null);
//             });
//
//             return;
//         }
//
//         if (StepProcessor.IsRunning)
//         {
//             return;
//         }
//
//         StepProcessor.Submit(() =>
//             Chain.Create()
//                 .Then(_ =>
//                 {
//                     var handler = Handlers[CurrentStep.Type];
//                     if (handler(module, vnav, lifestream))
//                     {
//                         stepIndex++;
//                     }
//                 })
//                 .Wait(1000 / 60)
//         );
//
//
//         if (stepIndex < Steps.Count)
//         {
//             return;
//         }
//
//         if (!module.config.RepeatCarrotHunt)
//         {
//             stopwatch.Stop();
//             running = false;
//         }
//
//         stepIndex = 0;
//         Steps.Clear();
//         vnav.Stop();
//         Plugin.Chain.Abort();
//         StepProcessor.Abort();
//         pathfinder = null;
//     }
//
//     public void Draw(CarrotsModule module)
//     {
//         if (!module.TryGetIPCProvider<VNavmesh>(out var vnav) || vnav == null)
//         {
//             return;
//         }
//
//         if (!module.TryGetIPCProvider<Lifestream>(out var lifestream) || lifestream == null)
//         {
//             return;
//         }
//
//         OcelotUI.Title($"{module.T("panel.hunt.title")}:");
//         OcelotUI.Indent(() =>
//         {
//             if (ImGui.Button(running ? I18N.T("generic.label.stop") : I18N.T("generic.label.start")))
//             {
//                 running = !running;
//                 if (running == false)
//                 {
//                     stopwatch.Stop();
//                     running = false;
//                     stepIndex = 0;
//                     Steps.Clear();
//                     vnav.Stop();
//                     Plugin.Chain.Abort();
//                     StepProcessor.Abort();
//                     pathfinder = null;
//                 }
//                 else
//                 {
//                     stopwatch.Restart();
//                 }
//             }
//
//             if (stopwatch.Elapsed > TimeSpan.Zero)
//             {
//                 ImGui.SameLine();
//                 if (ImGui.Button(module.T("panel.hunt.export.label")))
//                 {
//                     ClipboardService.SetText(JSON);
//                 }
//
//                 if (ImGui.IsItemHovered())
//                 {
//                     ImGui.SetTooltip(module.T("panel.hunt.export.tooltip"));
//                 }
//
//                 OcelotUI.LabelledValue(module.T("panel.hunt.elapsed"), $"{stopwatch.Elapsed:mm\\:ss}");
//             }
//
//
//             if (running && Steps.Count > 0)
//             {
//                 OcelotUI.LabelledValue(module.T("panel.hunt.instance.progress"), $"{stepIndex}/{Steps.Count}");
//
//                 if (CurrentStep.Type == PathfinderStepType.WalkToNode)
//                 {
//                     OcelotUI.LabelledValue(module.T("panel.hunt.distance_carrot"), $"{distance:f2}/{module.config.CarrotDetectionRange:f2}");
//                 }
//
//                 if (CurrentStep.Type == PathfinderStepType.WalkToAethernet)
//                 {
//                     OcelotUI.LabelledValue(module.T("panel.hunt.distance_shard"), $"{distance:f2}");
//                 }
//             }
//         });
//     }
// }



