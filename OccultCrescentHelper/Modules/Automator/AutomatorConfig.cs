using System.Collections.Generic;
using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace OccultCrescentHelper.Modules.Automator;

[Title("modules.automator.title")]
public partial class AutomatorConfig : ModuleConfig
{
    public override string ProviderNamespace {
        get => "OccultCrescentHelper.Modules.Automator";
    }

    [Checkbox]
    [Illegal]
    [RequiredPlugin("Lifestream", "vnavmesh")]
    [Label("generic.label.enabled")]
    [Tooltip("modules.automator.enabled.tooltip")]
    public bool Enabled { get; set; } = false;

    [Enum(typeof(AiType), nameof(AiTypeProvider))]
    [Label("modules.automator.ai_provider.label")]
    [Tooltip("modules.automator.ai_provider.tooltip")]
    public AiType AiProvider { get; set; } = AiType.VBM;

    [Checkbox]
    [Label("modules.automator.toggle_ai_provider.label")]
    [Tooltip("modules.automator.toggle_ai_provider.tooltip")]
    public bool ToggleAiProvider { get; set; } = true;

    public bool ShouldToggleAiProvider {
        get => IsPropertyEnabled(nameof(ToggleAiProvider));
    }

    [Checkbox]
    [Label("modules.automator.force_target.label")]
    [Tooltip("modules.automator.force_target.tooltip")]
    public bool ForceTarget { get; set; } = true;

    public bool ShouldForceTarget {
        get => IsPropertyEnabled(nameof(ForceTarget));
    }

    [Checkbox]
    [DependsOn(nameof(ForceTarget))]
    [Label("modules.automator.force_target_central_enemy.label")]
    [Tooltip("modules.automator.force_target_central_enemy.tooltip")]
    public bool ForceTargetCentralEnemy { get; set; } = true;

    public bool ShouldForceTargetCentralEnemy {
        get => IsPropertyEnabled(nameof(ForceTargetCentralEnemy));
    }

    [FloatRange(5f, 30f)]
    [Label("modules.automator.engagement_range.label")]
    [Tooltip("modules.automator.engagement_range.tooltip")]
    public float EngagementRange { get; set; } = 5f;

    // Critical Encounters
    [Checkbox]
    [Label("modules.automator.do_critical_encounters.label")]
    [Tooltip("modules.automator.do_critical_encounters.tooltip")]
    public bool DoCriticalEncounters { get; set; } = true;

    public bool ShouldDoCriticalEncounters {
        get => IsPropertyEnabled(nameof(DoCriticalEncounters));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Label("modules.automator.delay_critical_encounters.label")]
    [Tooltip("modules.automator.delay_critical_encounters.tooltip")]
    public bool DelayCriticalEncounters { get; set; } = false;

    public bool ShouldDelayCriticalEncounters {
        get => IsPropertyEnabled(nameof(DelayCriticalEncounters));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_scourge_of_the_mind.label")]
    [Tooltip("modules.automator.ce_scourge_of_the_mind.tooltip")]
    public bool DoScourgeOfTheMind { get; set; } = true;

    public bool ShouldDoScourgeOfTheMind {
        get => IsPropertyEnabled(nameof(DoScourgeOfTheMind));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_the_black_regiment.label")]
    [Tooltip("modules.automator.ce_the_black_regiment.tooltip")]
    public bool DoTheBlackRegiment { get; set; } = true;

    public bool ShouldDoTheBlackRegiment {
        get => IsPropertyEnabled(nameof(DoTheBlackRegiment));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_the_unbridled.label")]
    [Tooltip("modules.automator.ce_the_unbridled.tooltip")]
    public bool DoTheUnbridled { get; set; } = true;

    public bool ShouldDoTheUnbridled {
        get => IsPropertyEnabled(nameof(DoTheUnbridled));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_crawling_death.label")]
    [Tooltip("modules.automator.ce_crawling_death.tooltip")]
    public bool DoCrawlingDeath { get; set; } = true;

    public bool ShouldDoCrawlingDeath {
        get => IsPropertyEnabled(nameof(DoCrawlingDeath));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_calamity_bound.label")]
    [Tooltip("modules.automator.ce_calamity_bound.tooltip")]
    public bool DoCalamityBound { get; set; } = true;

    public bool ShouldDoCalamityBound {
        get => IsPropertyEnabled(nameof(DoCalamityBound));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_trial_by_claw.label")]
    [Tooltip("modules.automator.ce_trial_by_claw.tooltip")]
    public bool DoTrialByClaw { get; set; } = true;

    public bool ShouldDoTrialByClaw {
        get => IsPropertyEnabled(nameof(DoTrialByClaw));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_from_times_bygone.label")]
    [Tooltip("modules.automator.ce_from_times_bygone.tooltip")]
    public bool DoFromTimesBygone { get; set; } = true;

    public bool ShouldDoFromTimesBygone {
        get => IsPropertyEnabled(nameof(DoFromTimesBygone));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_company_of_stone.label")]
    [Tooltip("modules.automator.ce_company_of_stone.tooltip")]
    public bool DoCompanyOfStone { get; set; } = true;

    public bool ShouldDoCompanyOfStone {
        get => IsPropertyEnabled(nameof(DoCompanyOfStone));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_shark_attack.label")]
    [Tooltip("modules.automator.ce_shark_attack.tooltip")]
    public bool DoSharkAttack { get; set; } = true;

    public bool ShouldDoSharkAttack {
        get => IsPropertyEnabled(nameof(DoSharkAttack));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Label("modules.automator.ce_on_the_hunt.label")]
    [Tooltip("modules.automator.ce_on_the_hunt.tooltip")]
    public bool DoOnTheHunt { get; set; } = true;

    public bool ShouldDoOnTheHunt {
        get => IsPropertyEnabled(nameof(DoOnTheHunt));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_with_extreme_prejudice.label")]
    [Tooltip("modules.automator.ce_with_extreme_prejudice.tooltip")]
    public bool DoWithExtremePrejudice { get; set; } = true;

    public bool ShouldDoWithExtremePrejudice {
        get => IsPropertyEnabled(nameof(DoWithExtremePrejudice));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_noise_complaint.label")]
    [Tooltip("modules.automator.ce_noise_complaint.tooltip")]
    public bool DoNoiseComplaint { get; set; } = true;

    public bool ShouldDoNoiseComplaint {
        get => IsPropertyEnabled(nameof(DoNoiseComplaint));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_cursed_concern.label")]
    [Tooltip("modules.automator.ce_cursed_concern.tooltip")]
    public bool DoCursedConcern { get; set; } = true;

    public bool ShouldDoCursedConcern {
        get => IsPropertyEnabled(nameof(DoCursedConcern));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_eternal_watch.label")]
    [Tooltip("modules.automator.ce_eternal_watch.tooltip")]
    public bool DoEternalWatch { get; set; } = true;

    public bool ShouldDoEternalWatch {
        get => IsPropertyEnabled(nameof(DoEternalWatch));
    }

    [Checkbox]
    [DependsOn(nameof(DoCriticalEncounters))]
    [Indent]
    [Label("modules.automator.ce_flame_of_dusk.label")]
    [Tooltip("modules.automator.ce_flame_of_dusk.tooltip")]
    public bool DoFlameOfDusk { get; set; } = true;

    public bool ShouldDoFlameOfDusk {
        get => IsPropertyEnabled(nameof(DoFlameOfDusk));
    }

    // Fates
    [Checkbox]
    [Label("modules.automator.do_fates.label")]
    [Tooltip("modules.automator.do_fates.tooltip")]
    public bool DoFates { get; set; } = true;

    public bool ShouldDoFates {
        get => IsPropertyEnabled(nameof(DoFates));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_rough_waters.label")]
    [Tooltip("modules.automator.fate_rough_waters.tooltip")]
    public bool DoRoughWaters { get; set; } = true;

    public bool ShouldDoRoughWaters {
        get => IsPropertyEnabled(nameof(DoRoughWaters));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_the_golden_guardian.label")]
    [Tooltip("modules.automator.fate_the_golden_guardian.tooltip")]
    public bool DoTheGoldenGuardian { get; set; } = true;

    public bool ShouldDoTheGoldenGuardian {
        get => IsPropertyEnabled(nameof(DoTheGoldenGuardian));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_king_of_the_crescent.label")]
    [Tooltip("modules.automator.fate_king_of_the_crescent.tooltip")]
    public bool DoKingOfTheCrescent { get; set; } = true;

    public bool ShouldDoKingOfTheCrescent {
        get => IsPropertyEnabled(nameof(DoKingOfTheCrescent));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Experimental]
    [Label("modules.automator.fate_the_winged_terror.label")]
    [Tooltip("modules.automator.fate_the_winged_terror.tooltip")]
    public bool DoTheWingedTerror { get; set; } = false;

    public bool ShouldDoTheWingedTerror {
        get => IsPropertyEnabled(nameof(DoTheWingedTerror));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_an_unending_duty.label")]
    [Tooltip("modules.automator.fate_an_unending_duty.tooltip")]
    public bool DoAnUnendingDuty { get; set; } = true;

    public bool ShouldDoAnUnendingDuty {
        get => IsPropertyEnabled(nameof(DoAnUnendingDuty));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_brain_drain.label")]
    [Tooltip("modules.automator.fate_brain_drain.tooltip")]
    public bool DoBrainDrain { get; set; } = true;

    public bool ShouldDoBrainDrain {
        get => IsPropertyEnabled(nameof(DoBrainDrain));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_a_delicate_balance.label")]
    [Tooltip("modules.automator.fate_a_delicate_balance.tooltip")]
    public bool DoADelicateBalance { get; set; } = true;

    public bool ShouldDoADelicateBalance {
        get => IsPropertyEnabled(nameof(DoADelicateBalance));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_sworn_to_soil.label")]
    [Tooltip("modules.automator.fate_sworn_to_soil.tooltip")]
    public bool DoSwornToSoil { get; set; } = true;

    public bool ShouldDoSwornToSoil {
        get => IsPropertyEnabled(nameof(DoSwornToSoil));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_a_prying_eye.label")]
    [Tooltip("modules.automator.fate_a_prying_eye.tooltip")]
    public bool DoAPryingEye { get; set; } = true;

    public bool ShouldDoAPryingEye {
        get => IsPropertyEnabled(nameof(DoAPryingEye));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_fatal_allure.label")]
    [Tooltip("modules.automator.fate_fatal_allure.tooltip")]
    public bool DoFatalAllure { get; set; } = true;

    public bool ShouldDoFatalAllure {
        get => IsPropertyEnabled(nameof(DoFatalAllure));
    }

    [Checkbox]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_serving_darkness.label")]
    [Tooltip("modules.automator.fate_serving_darkness.tooltip")]
    public bool DoServingDarkness { get; set; } = true;

    public bool ShouldDoServingDarkness {
        get => IsPropertyEnabled(nameof(DoServingDarkness));
    }

    [Checkbox]
    [Experimental]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_persistent_pots.label")]
    [Tooltip("modules.automator.fate_persistent_pots.tooltip")]
    public bool DoPersistentPots { get; set; } = false;

    public bool ShouldDoPersistentPots {
        get => IsPropertyEnabled(nameof(DoPersistentPots));
    }

    [Checkbox]
    [Experimental]
    [Indent]
    [DependsOn(nameof(DoFates))]
    [Label("modules.automator.fate_pleading_pots.label")]
    [Tooltip("modules.automator.fate_pleading_pots.tooltip")]
    public bool DoPleadingPots { get; set; } = false;

    public bool ShouldDoPleadingPots {
        get => IsPropertyEnabled(nameof(DoPleadingPots));
    }

    public IReadOnlyDictionary<uint, bool> CriticalEncountersMap {
        get => new Dictionary<uint, bool> {
            { 33, ShouldDoScourgeOfTheMind },
            { 34, ShouldDoTheBlackRegiment },
            { 35, ShouldDoTheUnbridled },
            { 36, ShouldDoCrawlingDeath },
            { 37, ShouldDoCalamityBound },
            { 38, ShouldDoTrialByClaw },
            { 39, ShouldDoFromTimesBygone },
            { 40, ShouldDoCompanyOfStone },
            { 41, ShouldDoSharkAttack },
            { 42, ShouldDoOnTheHunt },
            { 43, ShouldDoWithExtremePrejudice },
            { 44, ShouldDoNoiseComplaint },
            { 45, ShouldDoCursedConcern },
            { 46, ShouldDoEternalWatch },
            { 47, ShouldDoFlameOfDusk },
        };
    }

    public IReadOnlyDictionary<uint, bool> FatesMap {
        get => new Dictionary<uint, bool> {
            { 1962, ShouldDoRoughWaters },
            { 1963, ShouldDoTheGoldenGuardian },
            { 1964, ShouldDoKingOfTheCrescent },
            { 1965, ShouldDoTheWingedTerror },
            { 1966, ShouldDoAnUnendingDuty },
            { 1967, ShouldDoBrainDrain },
            { 1968, ShouldDoADelicateBalance },
            { 1969, ShouldDoSwornToSoil },
            { 1970, ShouldDoAPryingEye },
            { 1971, ShouldDoFatalAllure },
            { 1972, ShouldDoServingDarkness },
            { 1976, ShouldDoPersistentPots },
            { 1977, ShouldDoPleadingPots },
        };
    }
}
