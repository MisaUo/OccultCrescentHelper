using System.Collections.Generic;
using BOCCHI.Enums;
using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.Fates;

[Title("modules.fates.title")]
public class FatesConfig : ModuleConfig
{
    [Checkbox]
    [Label("generic.label.enabled")]
    public bool Enabled { get; set; } = true;

    [Checkbox]
    [Label("modules.fates.config.alert_all.label")]
    [Tooltip("modules.fates.config.alert_all.tooltip")]
    public bool AlertAll { get; set; } = false;

    [Checkbox]
    [Label("modules.fates.config.alert_azurite.label")]
    [Tooltip("modules.fates.config.alert_azurite.tooltip")]
    public bool AlertAzurite { get; set; } = false;

    [Checkbox]
    [Label("modules.fates.config.alert_verdigris.label")]
    [Tooltip("modules.fates.config.alert_verdigris.tooltip")]
    public bool AlertVerdigris { get; set; } = false;

    [Checkbox]
    [Label("modules.fates.config.alert_malachite.label")]
    [Tooltip("modules.fates.config.alert_malachite.tooltip")]
    public bool AlertMalachite { get; set; } = false;

    [Checkbox]
    [Label("modules.fates.config.alert_realgar.label")]
    [Tooltip("modules.fates.config.alert_realgar.tooltip")]
    public bool AlertRealgar { get; set; } = false;

    [Checkbox]
    [Label("modules.fates.config.alert_caputmortuum.label")]
    [Tooltip("modules.fates.config.alert_caputmortuum.tooltip")]
    public bool AlertCaputMortuum { get; set; } = false;

    [Checkbox]
    [Label("modules.fates.config.alert_orpiment.label")]
    [Tooltip("modules.fates.config.alert_orpiment.tooltip")]
    public bool AlertOrpiment { get; set; } = false;
}
