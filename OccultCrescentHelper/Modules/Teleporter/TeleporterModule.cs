using ImGuiNET;
using OccultCrescentHelper.Modules.StateManager;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.Teleporter;

[OcelotModule(1)]
public class TeleporterModule : Module<Plugin, Config>
{
    public override TeleporterConfig config {
        get => _config.TeleporterConfig;
    }

    public readonly Teleporter teleporter;

    public TeleporterModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        teleporter = new Teleporter(this);
    }

    public override void Initialize()
    {
        if (TryGetModule<StateManagerModule>(out var states) && states != null)
        {
            states.OnExitInFate += teleporter.OnFateEnd;
            states.OnExitInCriticalEncounter += teleporter.OnCriticalEncounterEnd;
        }
    }

    public override void Dispose()
    {
        if (TryGetModule<StateManagerModule>(out var states) && states != null)
        {
            states.OnExitInFate -= teleporter.OnFateEnd;
            states.OnExitInCriticalEncounter -= teleporter.OnCriticalEncounterEnd;
        }
    }

    public bool IsReady()
    {
        return teleporter.IsReady();
    }
}
