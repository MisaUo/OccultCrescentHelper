using System;
using Ocelot.Config.Attributes;
using Ocelot.Modules;

namespace BOCCHI.Modules.EventDrop;

[Serializable]
[Title]
public class EventDropConfig : ModuleConfig
{
    [Checkbox] public bool ShowDemiatmaDrops { get; set; } = true;

    [Checkbox] public bool ShowNoteDrops { get; set; } = true;

    [Checkbox] public bool ShowSoulShardDrops { get; set; } = true;
}
