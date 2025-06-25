using System;
using BOCCHI.Modules.Automator;
using BOCCHI.Modules.Buff;
using BOCCHI.Modules.Carrots;
using BOCCHI.Modules.CriticalEncounters;
using BOCCHI.Modules.Currency;
using BOCCHI.Modules.EventDrop;
using BOCCHI.Modules.Exp;
using BOCCHI.Modules.Fates;
using BOCCHI.Modules.InstanceIdentifier;
using BOCCHI.Modules.Mount;
using BOCCHI.Modules.StateManager;
using BOCCHI.Modules.Teleporter;
using BOCCHI.Modules.Treasure;
using BOCCHI.Modules.WindowManager;
using ECommons.DalamudServices;
using Ocelot;

namespace BOCCHI;

[Serializable]
public class Config : IOcelotConfig
{
    public TreasureConfig TreasureConfig { get; set; } = new();

    public CarrotsConfig CarrotsConfig { get; set; } = new();

    public CurrencyConfig CurrencyConfig { get; set; } = new();

    public BuffConfig BuffConfig { get; set; } = new();

    public EventDropConfig EventDropConfig { get; set; } = new();

    public FatesConfig FatesConfig { get; set; } = new();

    public CriticalEncountersConfig CriticalEncountersConfig { get; set; } = new();

    public ExpConfig ExpConfig { get; set; } = new();

    public TeleporterConfig TeleporterConfig { get; set; } = new();

    public WindowManagerConfig WindowManagerConfig { get; set; } = new();

    public StateManagerConfig StateManagerConfig { get; set; } = new();

    public InstanceIdentifierConfig InstanceIdentifierConfig { get; set; } = new();

    public AutomatorConfig AutomatorConfig { get; set; } = new();

    public MountConfig MountConfig { get; set; } = new();

    public int Version { get; set; } = 1;

    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
