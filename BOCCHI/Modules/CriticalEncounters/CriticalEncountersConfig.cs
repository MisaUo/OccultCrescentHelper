using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.CriticalEncounters;

[Title("modules.critical_encounters.title")]
public class CriticalEncountersConfig : ModuleConfig
{
    [Checkbox]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = true;

    [Checkbox]
    [Label("modules.critical_encounters.config.track_forked_tower.label")]
    [Tooltip("modules.critical_encounters.config.track_forked_tower.tooltip")]
    public bool TrackForkedTower { get; set; } = true;

    [Checkbox]
    [Label("modules.critical_encounters.config.log_spawn.label")]
    [Tooltip("modules.critical_encounters.config.log_spawn.tooltip")]
    public bool LogSpawn { get; set; } = false;

    [Checkbox]
    [Label("modules.critical_encounters.config.alert_all.label")]
    [Tooltip("modules.critical_encounters.config.alert_all.tooltip")]
    public bool AlertAll { get; set; } = false;

    [Checkbox]
    [Label("modules.critical_encounters.config.alert_azurite.label")]
    [Tooltip("modules.critical_encounters.config.alert_azurite.tooltip")]
    public bool AlertAzurite { get; set; } = false;

    [Checkbox]
    [Label("modules.critical_encounters.config.alert_verdigris.label")]
    [Tooltip("modules.critical_encounters.config.alert_verdigris.tooltip")]
    public bool AlertVerdigris { get; set; } = false;

    [Checkbox]
    [Label("modules.critical_encounters.config.alert_malachite.label")]
    [Tooltip("modules.critical_encounters.config.alert_malachite.tooltip")]
    public bool AlertMalachite { get; set; } = false;

    [Checkbox]
    [Label("modules.critical_encounters.config.alert_realgar.label")]
    [Tooltip("modules.critical_encounters.config.alert_realgar.tooltip")]
    public bool AlertRealgar { get; set; } = false;

    [Checkbox]
    [Label("modules.critical_encounters.config.alert_caputmortuum.label")]
    [Tooltip("modules.critical_encounters.config.alert_caputmortuum.tooltip")]
    public bool AlertCaputMortuum { get; set; } = false;

    [Checkbox]
    [Label("modules.critical_encounters.config.alert_orpiment.label")]
    [Tooltip("modules.critical_encounters.config.alert_orpiment.tooltip")]
    public bool AlertOrpiment { get; set; } = false;

    [Checkbox]
    [Label("modules.critical_encounters.config.alert_oracle.label")]
    [Tooltip("modules.critical_encounters.config.alert_oracle.tooltip")]
    public bool AlertOracle { get; set; } = false;

    [Checkbox]
    [Label("modules.critical_encounters.config.alert_berserker.label")]
    [Tooltip("modules.critical_encounters.config.alert_berserker.tooltip")]
    public bool AlertBerserker { get; set; } = false;

    [Checkbox]
    [Label("modules.critical_encounters.config.alert_ranger.label")]
    [Tooltip("modules.critical_encounters.config.alert_ranger.tooltip")]
    public bool AlertRanger { get; set; } = false;
}
