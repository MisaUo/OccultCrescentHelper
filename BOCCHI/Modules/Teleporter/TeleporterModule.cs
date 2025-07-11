using BOCCHI.Data;
using BOCCHI.Modules.StateManager;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Ocelot.Modules;

namespace BOCCHI.Modules.Teleporter;

[OcelotModule(2)]
public class TeleporterModule : Module<Plugin, Config>
{
    public override TeleporterConfig config
    {
        get => _config.TeleporterConfig;
    }

    public readonly Teleporter teleporter;

    public TeleporterModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        teleporter = new Teleporter(this);

        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectYesno", OnSelectYesnoPostSetup);
    }

    public override void Initialize()
    {
        var states = GetModule<StateManagerModule>();
        states.OnExitInFate += teleporter.OnFateEnd;
        states.OnExitInCriticalEncounter += teleporter.OnCriticalEncounterEnd;
    }

    public override void Dispose()
    {
        var states = GetModule<StateManagerModule>();
        states.OnExitInFate -= teleporter.OnFateEnd;
        states.OnExitInCriticalEncounter -= teleporter.OnCriticalEncounterEnd;

        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "SelectYesno", OnSelectYesnoPostSetup);
    }

    public bool IsReady()
    {
        return teleporter.IsReady();
    }

    private unsafe void OnSelectYesnoPostSetup(AddonEvent type, AddonArgs args)
    {
        if (!ZoneData.IsInOccultCrescent() || ZoneData.IsInForkedTower() || Player.IsDead)
        {
            return;
        }

        var addon = (AtkUnitBase*)args.Addon;
        if (!addon->IsVisible)
        {
            return;
        }

        // This could be the dumbest thing I've ever written, but that bar is low
        if (addon->AtkValues[7].Type != ValueType.Int || addon->AtkValues[7].Int != -1)
        {
            return;
        }

        addon->FireCallbackInt(0);
    }
}
