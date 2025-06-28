using System;
using System.IO;
using BOCCHI.Chains;
using BOCCHI.Data;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using ECommons;
using ECommons.DalamudServices;
using ECommons.Reflection;
using Ocelot;
using Ocelot.Chain;

namespace BOCCHI;

public sealed class Plugin : OcelotPlugin
{
    public override string Name
    {
        get => "Occult Crescent Helper";
    }

    public Config config { get; init; }

    public override IOcelotConfig _config
    {
        get => config;
    }

    public static ChainQueue Chain
    {
        get => ChainManager.Get("OCH##main");
    }

    public Plugin(IDalamudPluginInterface plugin)
        : base(plugin, Module.DalamudReflector)
    {
        config = plugin.GetPluginConfig() as Config ?? new Config();

        I18N.SetDirectory(plugin.AssemblyLocation.Directory?.FullName!);
        I18N.LoadFromFile("en", "Translations/en.json");
        I18N.LoadFromFile("uwu", "Translations/uwu.json");

        if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1 && new Random().NextDouble() < 0.05)
        {
            I18N.SetLanguage("uwu");
        }

        InitializeClientStructs();
        OcelotInitialize();

        ChainManager.Initialize();
        ChainHelper.Initialize(this);
    }

    private void InitializeClientStructs()
    {
        var gameVersion = DalamudReflector.TryGetDalamudStartInfo(out var ver) ? ver.GameVersion!.ToString() : "unknown";
        InteropGenerator.Runtime.Resolver.GetInstance.Setup(
            Svc.SigScanner.SearchBase,
            gameVersion,
            new FileInfo(Svc.PluginInterface.ConfigDirectory.FullName + "/cs.json")
        );
        FFXIVClientStructs.Interop.Generated.Addresses.Register();
        InteropGenerator.Runtime.Resolver.GetInstance.Resolve();
    }


    public override bool ShouldTick()
    {
        return ZoneData.IsInOccultCrescent()
               && !(
                   Svc.Condition[ConditionFlag.BetweenAreas] ||
                   Svc.Condition[ConditionFlag.BetweenAreas51] ||
                   Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                   Svc.Condition[ConditionFlag.OccupiedInEvent] ||
                   Svc.Condition[ConditionFlag.WatchingCutscene] ||
                   Svc.Condition[ConditionFlag.WatchingCutscene78] ||
                   Svc.ClientState.LocalPlayer?.IsTargetable != true
               );
    }

    public override void Dispose()
    {
        base.Dispose();
        ChainManager.Close();
    }
}
