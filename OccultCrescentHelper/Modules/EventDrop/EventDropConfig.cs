using System;
using Ocelot.Config.Attributes;

namespace BOCCHI.Modules.EventDrop;

[Serializable]
[Title("modules.event_drop.title")]
public class EventDropConfig : Ocelot.Modules.ModuleConfig
{
    [Checkbox]
    [Label("modules.event_drop.demiatma.label")]
    [Tooltip("modules.event_drop.demiatma.tooltip")]
    public bool ShowDemiatmaDrops { get; set; } = true;

    [Checkbox]
    [Label("modules.event_drop.notes.label")]
    [Tooltip("modules.event_drop.notes.tooltip")]
    public bool ShowNoteDrops { get; set; } = true;

    [Checkbox]
    [Label("modules.event_drop.soulshards.label")]
    [Tooltip("modules.event_drop.soulshards.tooltip")]
    public bool ShowSoulShardDrops { get; set; } = true;
}
