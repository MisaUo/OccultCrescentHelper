using System.Collections.Generic;
using System.Linq;
using BOCCHI.Data.Traps;
using BOCCHI.Enums;
using BOCCHI.Modules.Data;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Colors;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ImGuiNET;
using Ocelot.Modules;
using Ocelot.Windows;
using Pictomancy;

namespace BOCCHI.Modules.ForkedTower;

public class TowerRun(string hash)
{
    public readonly string Hash = hash;

    private readonly Dictionary<string, IEventObj> DiscoveredTraps = [];

    private readonly Dictionary<string, TrackedGroup> TrackedGroups = [];

    public bool HasDiscoveredAllTraps(TrapGroup group)
    {
        if (TrackedGroups.TryGetValue(group.GetKey(), out var trackedGroup))
        {
            return trackedGroup.HasDiscoveredAllTraps();
        }

        return false;
    }

    public void Update(UpdateContext context)
    {
        foreach (var trap in GetNearbyTraps())
        {
            if (!DiscoveredTraps.TryAdd(trap.GetKey(), trap))
            {
                continue;
            }

            var group = TrapData.GetGroup(trap);

            if (!TrackedGroups.TryGetValue(group.GetKey(), out var trackedGroup))
            {
                trackedGroup = new TrackedGroup(group);
                TrackedGroups.Add(group.GetKey(), trackedGroup);
            }

            trackedGroup.Traps.Add(trap);
        }
    }

    public void Render(RenderContext context)
    {
        using var pictomancy = PictoService.Draw();
        if (pictomancy == null)
        {
            return;
        }

        if (context.Config is not Config config)
        {
            return;
        }

        foreach (var trap in DiscoveredTraps.Values)
        {
            if (Player.DistanceTo(trap) > config.ForkedTowerConfig.TrapDrawRange)
            {
                continue;
            }

            if (config.ForkedTowerConfig.DrawSmallTrapRange && trap.DataId == (uint)OccultObjectType.Trap)
            {
                pictomancy.AddCircle(trap.Position, 6f, ImGui.GetColorU32(ImGuiColors.DPSRed));
            }

            if (config.ForkedTowerConfig.DrawBigTrapRange && trap.DataId == (uint)OccultObjectType.BigTrap)
            {
                pictomancy.AddCircle(trap.Position, 30f, ImGui.GetColorU32(ImGuiColors.DPSRed));
            }
        }
    }

    private IEnumerable<IEventObj> GetNearbyTraps()
    {
        return Svc.Objects.OfType<IEventObj>().Where(o => o.DataId is (uint)OccultObjectType.Trap or (uint)OccultObjectType.BigTrap);
    }
}
