using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using BOCCHI.Data.Traps;
using BOCCHI.Enums;
using BOCCHI.Modules.CriticalEncounters;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using ImGuiNET;
using Ocelot.Modules;
using Ocelot.Windows;
using Pictomancy;

namespace BOCCHI.Modules.ForkedTower;

[OcelotModule]
public class ForkedTowerModule(Plugin plugin, Config config) : Module(plugin, config)
{
    public override ForkedTowerConfig Config
    {
        get => PluginConfig.ForkedTowerConfig;
    }

    public TowerRun TowerRun { get; private set; } = new("");

    private readonly Panel panel = new();

    public override void PostInitialize()
    {
        GetModule<CriticalEncountersModule>().Tracker.OnBattleState += OnCriticalEncounterBattle;

        StartNewRun();
    }

    public override void Update(UpdateContext context)
    {
        TowerRun.Update(context);
    }

    public override void Render(RenderContext context)
    {
        if (!Config.DrawPotentialTrapPositions)
        {
            return;
        }

        using var pictomancy = PictoService.Draw();
        if (pictomancy == null)
        {
            return;
        }

        var traps = GetTrapsToRender().ToList();
        foreach (var trap in traps)
        {
            if (Config.DrawSimpleMode || Config.DrawOutlineForComplexMode)
            {
                pictomancy.AddCircle(trap.Position, 4f, ImGui.GetColorU32(GetTrapColor(trap.Type)));
            }

            if (!Config.DrawSimpleMode)
            {
                var key = $"{trap.Position.X:f2}:{trap.Position.Y:f2}:{trap.Position.Z:f2}.{trap.Type}";
                PictoService.VfxRenderer.AddCircle(key, trap.Position, 4f, GetTrapColor(trap.Type));
            }
        }

        TowerRun.Render(context);
    }

    private Vector4 GetTrapColor(OccultObjectType type)
    {
        return type switch
        {
            OccultObjectType.Trap => Config.TrapDrawColor,
            OccultObjectType.BigTrap => Config.BigTrapDrawColor,
            _ => new Vector4(4f, 7f, 1f, 1f),
        };
    }


    public override bool RenderMainUi(RenderContext context)
    {
        panel.Draw(this);
        return true;
    }

    private IEnumerable<TrapDatum> GetTrapsToRender()
    {
        var groups = TrapData.Groups.AsEnumerable();

#if !DEBUG
        groups = groups.Where(group => group.GetDistance() <= Config.TrapDrawRange);
#endif

        if (Config.StopRenderingCompleteGroups)
        {
            groups = groups.Where(group => !TowerRun.HasDiscoveredAllTraps(group));
        }

        return groups.SelectMany(group => group.Traps);
    }

    private void OnCriticalEncounterBattle(DynamicEvent ev)
    {
        if (ev.EventType < 4)
        {
            return;
        }

        StartNewRun();
    }

    private void StartNewRun()
    {
        TowerRun = new TowerRun(GenerateHash());
    }

    private string GenerateHash()
    {
        using var sha256 = SHA256.Create();

        var timeBytes = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        var contentIdBytes = BitConverter.GetBytes(Player.CID);

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeBytes);
            Array.Reverse(contentIdBytes);
        }

        var combined = new byte[timeBytes.Length + contentIdBytes.Length];
        Buffer.BlockCopy(timeBytes, 0, combined, 0, timeBytes.Length);
        Buffer.BlockCopy(contentIdBytes, 0, combined, timeBytes.Length, contentIdBytes.Length);

        var hashBytes = sha256.ComputeHash(combined);

        return Convert.ToBase64String(hashBytes);
    }

    public override void Dispose()
    {
        GetModule<CriticalEncountersModule>().Tracker.OnBattleState -= OnCriticalEncounterBattle;
    }
}
