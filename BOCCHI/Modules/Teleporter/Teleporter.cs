using System.Linq;
using System.Numerics;
using BOCCHI.Chains;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.Automator;
using Dalamud.Interface;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ImGuiNET;
using Ocelot;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;

namespace BOCCHI.Modules.Teleporter;

public class Teleporter
{
    private readonly TeleporterModule module;

    public Teleporter(TeleporterModule module)
    {
        this.module = module;
    }

    public void Button(Aethernet? aethernet, Vector3 destination, string name, string id, EventData ev)
    {
        if (!module.TryGetIPCProvider<VNavmesh>(out var vnav) || vnav == null || !vnav.IsReady())
        {
            return;
        }

        if (aethernet == null)
        {
            aethernet = ZoneHelper.GetClosestAethernetShard(destination);
        }

        OcelotUI.Indent(() =>
        {
            PathfindingButton(destination, name, id, ev);
            TeleportButton((Aethernet)aethernet, destination, name, id, ev);
        });
    }

    private void PathfindingButton(Vector3 destination, string name, string id, EventData ev)
    {
        if (!module.TryGetIPCProvider<VNavmesh>(out var vnav) || vnav == null || !vnav.IsReady())
        {
            return;
        }

        if (ImGuiEx.IconButton(FontAwesomeIcon.Running, $"{name}##{id}"))
        {
            Svc.Log.Info($"Pathfinding to {name} at {destination}");

            Plugin.Chain.Submit(() => Chain.Create("Pathfinding")
                .Then(new PathfindingChain(vnav, destination, ev, 20f))
                .ConditionalThen(_ => module.config.ShouldMount, ChainHelper.MountChain())
                .WaitUntilNear(vnav, destination, 205f)
            );
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"Pathfind to {name}");
        }

        if (!module.TryGetIPCProvider<Lifestream>(out var lifestream) || lifestream == null || !lifestream.IsReady())
        {
            return;
        }

        ImGui.SameLine();
    }

    private void TeleportButton(Aethernet aethernet, Vector3 destination, string name, string id, EventData ev)
    {
        if (!module.TryGetIPCProvider<Lifestream>(out var lifestream) || lifestream == null || !lifestream.IsReady())
        {
            return;
        }

        var isNearShards = ZoneHelper.GetNearbyAethernetShards().Count() > 0;
        var isNearCurrentShard = ZoneHelper.IsNearAethernetShard(aethernet);

        if (ImGuiEx.IconButton(FontAwesomeIcon.LocationArrow, $"{name}##{id}", enabled: isNearShards && !isNearCurrentShard))
        {
            var factory = () =>
            {
                var chain = Chain.Create("Teleport Sequence")
                    .Then(ChainHelper.TeleportChain(aethernet))
                    .Debug("Waiting for lifestream to not be 'busy'")
                    .Then(new TaskManagerTask(() => !lifestream.IsBusy(), new TaskManagerConfiguration { TimeLimitMS = 30000 }));

                if (module.TryGetIPCProvider<VNavmesh>(out var vnav) && vnav != null && vnav.IsReady())
                {
                    chain
                        .RunIf(() => module.config.PathToDestination)
                        .Then(new PathfindingChain(vnav, destination, ev, 20f))
                        .ConditionalThen(_ => module.config.ShouldMount, ChainHelper.MountChain())
                        .WaitUntilNear(vnav, destination, 20f);
                }

                return chain;
            };

            Plugin.Chain.Submit(factory);
        }

        if (ImGui.IsItemHovered())
        {
            if (!isNearShards)
            {
                ImGui.SetTooltip($"You must be near an aetheryte to teleport");
            }
            else if (isNearCurrentShard)
            {
                ImGui.SetTooltip($"You're already at this aetheryte");
            }
            else
            {
                ImGui.SetTooltip($"Teleport to {aethernet.ToFriendlyString()}");
            }
        }
    }

    public void OnFateEnd()
    {
        if (module.GetModule<AutomatorModule>().enabled)
        {
            return;
        }

        if (!module.config.ReturnAfterFate)
        {
            return;
        }

        Return();
    }

    public void OnCriticalEncounterEnd()
    {
        if (module.GetModule<AutomatorModule>().enabled)
        {
            return;
        }

        if (!module.config.ReturnAfterCriticalEncounter)
        {
            return;
        }

        Return();
    }

    public void Return()
    {
        if (ZoneData.IsInForkedTower())
        {
            return;
        }

        var player = Svc.ClientState.LocalPlayer;
        if (player == null)
        {
            return;
        }

        Plugin.Chain.Submit(ChainHelper.ReturnChain());
    }

    public bool IsReady()
    {
        return module.TryGetIPCProvider<Lifestream>(out _);
    }
}
