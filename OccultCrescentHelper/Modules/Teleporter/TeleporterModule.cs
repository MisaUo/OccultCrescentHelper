using BOCCHI.Modules.StateManager;
using Ocelot.Modules;

namespace BOCCHI.Modules.Teleporter;

[OcelotModule(1)]
public class TeleporterModule : Module<Plugin, Config>
{
    public readonly Teleporter teleporter;

    public TeleporterModule(Plugin plugin, Config config)
        : base(plugin, config)
    {
        teleporter = new Teleporter(this);
    }

    public override TeleporterConfig config
    {
        get => _config.TeleporterConfig;
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
