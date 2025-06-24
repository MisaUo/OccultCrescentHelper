using System;
using System.IO;
using BOCCHI.Chains;
using BOCCHI.Data;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using DotNetEnv;
using ECommons;
using ECommons.DalamudServices;
using ECommons.Reflection;
using FFXIVClientStructs.Interop.Generated;
using InteropGenerator.Runtime;
using Ocelot;
using Ocelot.Chain;

namespace BOCCHI;

public sealed class Plugin : OcelotPlugin
{
    public Plugin(IDalamudPluginInterface plugin)
        : base(plugin, Module.DalamudReflector)
    {
        Config = plugin.GetPluginConfig() as Config ?? new Config();

        I18N.SetDirectory(plugin.AssemblyLocation.Directory?.FullName!);
        I18N.LoadFromFile("en", "Translations/en.json");
        I18N.LoadFromFile("uwu", "Translations/uwu.json");

        if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1 && new Random().NextDouble() < 0.05)
            I18N.SetLanguage("uwu");

        InitializeClientStructs();
        OcelotInitialize();

        ChainManager.Initialize();
        ChainHelper.Initialize(this);

        Env.Load(Svc.PluginInterface.AssemblyLocation.Directory + "/.env");
    }

    public override string Name => "Occult Crescent Helper";

    public Config Config { get; init; }

    public override IOcelotConfig _config => Config;

    public static ChainQueue Chain => ChainManager.Get("OCH##main");

    private void InitializeClientStructs()
    {
        var gameVersion = DalamudReflector.TryGetDalamudStartInfo(out var ver)
                              ? ver.GameVersion!.ToString()
                              : "unknown";
        Resolver.GetInstance.Setup(
            Svc.SigScanner.SearchBase,
            gameVersion,
            new FileInfo(Svc.PluginInterface.ConfigDirectory.FullName + "/cs.json")
        );
        Addresses.Register();
        Resolver.GetInstance.Resolve();
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
