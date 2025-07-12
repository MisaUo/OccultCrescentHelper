using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using BOCCHI.Enums;
using BOCCHI.Modules.CriticalEncounters;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Ocelot.Modules;
using Pictomancy;

namespace BOCCHI.Modules.ForkedTower;

[OcelotModule]
public class ForkedTowerModule(Plugin plugin, Config config) : Module<Plugin, Config>(plugin, config)
{
    public override ForkedTowerConfig config
    {
        get => _config.ForkedTowerConfig;
    }

    public string TowerHash { get; private set; } = "";

    private readonly Panel panel = new();

    private List<(Vector3 Position, OccultObjectType Type)> TrapPositions { get; set; } = [];

    public override void PostInitialize()
    {
        GetModule<CriticalEncountersModule>().Tracker.OnBattleState += OnCriticalEncounterBattle;

        GenerateHash();
    }

    public override void Draw()
    {
        if (!config.DrawPotentialTrapPositions)
        {
            return;
        }

        foreach (var trap in TrapPositions.Where(t => Player.DistanceTo(t.Position) <= config.TrapDrawRange))
        {
            var key = $"{trap.Position.X:f2}:{trap.Position.Y:f2}:{trap.Position.Z:f2}.{trap.Type}";
            PictoService.VfxRenderer.AddCircle(key, trap.Position, 4f, GetTrapColor(trap.Type));
        }
    }

    private Vector4 GetTrapColor(OccultObjectType type)
    {
        return type switch
        {
            OccultObjectType.Trap => config.TrapDrawColor,
            OccultObjectType.BigTrap => config.BigTrapDrawColor,
            _ => new Vector4(4f, 7f, 1f, 1f),
        };
    }


    public override bool DrawMainUi()
    {
        panel.Draw(this);
        return true;
    }

    private void OnCriticalEncounterBattle(DynamicEvent ev)
    {
        if (ev.EventType < 4)
        {
            return;
        }

        GenerateHash();
    }

    private void GenerateHash()
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

        TowerHash = Convert.ToBase64String(hashBytes);
        Svc.Log.Debug($"New Tower Hash Generated {TowerHash}");
    }

    public override void Dispose()
    {
        GetModule<CriticalEncountersModule>().Tracker.OnBattleState -= OnCriticalEncounterBattle;
    }
}
