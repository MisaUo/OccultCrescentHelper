﻿using BOCCHI.Chains;
using BOCCHI.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using ECommons;
using ECommons.DalamudServices;
using Ocelot;
using Ocelot.Chain;
using System;

namespace BOCCHI;

public sealed class Plugin : OcelotPlugin
{
    public override string Name
    {
        get => "Occult Crescent Helper";
    }

    public Config Config { get; }

    public override IOcelotConfig OcelotConfig
    {
        get => Config;
    }

    public static ChainQueue Chain
    {
        get => ChainManager.Get("OCH##main");
    }
    private bool IsDev;
    public Plugin(IDalamudPluginInterface plugin)
        : base(plugin, Module.DalamudReflector)
    {
#if !DEBUG
        if (plugin.IsDev)
        {
            IsDev = true;
            return;
        }
#endif
        Config = plugin.GetPluginConfig() as Config ?? new Config();

        SetupLanguage(plugin);

        OcelotInitialize();

        ChainHelper.Initialize(this);
    }

    private void SetupLanguage(IDalamudPluginInterface plugin)
    {
        I18N.SetDirectory(plugin.AssemblyLocation.Directory?.FullName!);
        I18N.LoadAllFromDirectory("en", "Translations/en");
        I18N.LoadAllFromDirectory("jp", "Translations/jp");
        I18N.LoadAllFromDirectory("fr", "Translations/fr");
        I18N.LoadAllFromDirectory("zh", "Translations/zh");

        // @todo: Breakup German and uwu translation
        I18N.LoadFromFile("de", "Translations/de.json");
        I18N.LoadFromFile("uwu", "Translations/uwu.json");

        var lang = Svc.ClientState.ClientLanguage switch
        {
            ClientLanguage.French => "fr",
            ClientLanguage.German => "de",
            ClientLanguage.Japanese => "jp",
            ClientLanguage.ChineseSimplified => "zh",
            _ => "en",
        };

        I18N.SetLanguage(lang);

        var today = DateTime.Today;
        if (today is { Month: 4, Day: 1 } && Random.Shared.NextDouble() < 0.05)
        {
            I18N.SetLanguage("uwu");
        }
    }

    protected override bool ShouldUpdate()
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
}
