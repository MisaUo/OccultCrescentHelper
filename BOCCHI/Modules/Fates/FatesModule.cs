using System.Collections.Generic;
using BOCCHI.Data;
using Dalamud.Game.ClientState.Fates;
using Ocelot.Modules;
using Ocelot.Windows;

namespace BOCCHI.Modules.Fates;

[OcelotModule(1001, 5)]
public class FatesModule : Module
{
    public override FatesConfig Config
    {
        get => PluginConfig.FatesConfig;
    }

    public override bool IsEnabled
    {
        get => Config.IsPropertyEnabled(nameof(Config.Enabled));
    }

    public override bool ShouldUpdate
    {
        get => true;
    }

    public readonly FateTracker tracker = new();

    public Dictionary<uint, IFate> fates
    {
        get => tracker.Fates;
    }

    public Dictionary<uint, EventProgress> progress
    {
        get => tracker.Progress;
    }

    private readonly Panel panel = new();

    private readonly Alerter alerter;

    public FatesModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        alerter = new Alerter(this);
    }

    public override void Update(UpdateContext context)
    {
        tracker.Tick(context.Framework);
    }

    public override bool RenderMainUi(RenderContext context)
    {
        panel.Draw(this);
        return true;
    }

    public override void OnTerritoryChanged(ushort id)
    {
        fates.Clear();
    }

    public override void Dispose()
    {
        base.Dispose();
        alerter.Dispose();
    }
}
