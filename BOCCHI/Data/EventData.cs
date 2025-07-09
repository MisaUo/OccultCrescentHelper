using System.Collections.Generic;
using System.Numerics;
using BOCCHI.Enums;

namespace BOCCHI.Data;

public struct EventData
{
    public uint id;

    public EventType type;

    public string Name;

    public Demiatma? demiatma;

    public SoulShard? soulshard;

    public MonsterNote? notes;

    public Monster? monster;

    public Aethernet? aethernet;

    public Vector3? start;

    public float? radius;

    public readonly static Dictionary<uint, EventData> Fates = new()
    {
        {
            1962,
            new EventData
            {
                id = 1962,
                type = EventType.Fate,
                Name = "Rough Waters",
                demiatma = Demiatma.Azurite,
                monster = Monster.Nammu,
                start = new Vector3(162.00f, 56.00f, 676.00f),
            }
        },
        {
            1963,
            new EventData
            {
                id = 1963,
                type = EventType.Fate,
                Name = "The Golden Guardian",
                demiatma = Demiatma.Azurite,
                monster = Monster.GildedHeadstone,
                start = new Vector3(373.20f, 70.00f, 486.00f),
            }
        },
        {
            1964,
            new EventData
            {
                id = 1964,
                type = EventType.Fate,
                Name = "King of the Crescent",
                demiatma = Demiatma.Orpiment,
                monster = Monster.Ropross,
                start = new Vector3(-226.10f, 116.38f, 254.00f),
            }
        },
        {
            1965,
            new EventData
            {
                id = 1965,
                type = EventType.Fate,
                Name = "The Winged Terror",
                demiatma = Demiatma.Realgar,
                monster = Monster.GiantBird,
                aethernet = Aethernet.TheWanderersHaven,
                start = new Vector3(-548.50f, 3.00f, -595.00f),
            }
        },
        {
            1966,
            new EventData
            {
                id = 1966,
                type = EventType.Fate,
                Name = "An Unending Duty",
                demiatma = Demiatma.Malachite,
                monster = Monster.Sisyphus,
                start = new Vector3(-223.10f, 107.00f, 36.00f),
            }
        },
        {
            1967,
            new EventData
            {
                id = 1967,
                type = EventType.Fate,
                Name = "Brain Drain",
                demiatma = Demiatma.Realgar,
                monster = Monster.AdvancedAevis,
                aethernet = Aethernet.CrystallizedCaverns,
                start = new Vector3(-48.10f, 111.76f, -320.00f),
            }
        },
        {
            1968,
            new EventData
            {
                id = 1968,
                type = EventType.Fate,
                Name = "A Delicate Balance",
                demiatma = Demiatma.Verdigris,
                monster = Monster.Dehumidifier,
                start = new Vector3(-370.00f, 75.00f, 650.00f),
            }
        },
        {
            1969,
            new EventData
            {
                id = 1969,
                type = EventType.Fate,
                Name = "Sworn to Soil",
                demiatma = Demiatma.Verdigris,
                monster = Monster.MadMudarch,
                start = new Vector3(-589.10f, 96.50f, 333.00f),
            }
        },
        {
            1970,
            new EventData
            {
                id = 1970,
                type = EventType.Fate,
                Name = "A Prying Eye",
                demiatma = Demiatma.Azurite,
                monster = Monster.Observer,
                start = new Vector3(-71.00f, 71.31f, 557.00f),
            }
        },
        {
            1971,
            new EventData
            {
                id = 1971,
                type = EventType.Fate,
                Name = "Fatal Allure",
                demiatma = Demiatma.Orpiment,
                monster = Monster.Execrator,
                start = new Vector3(79.00f, 97.86f, 278.00f),
            }
        },
        {
            1972,
            new EventData
            {
                id = 1972,
                type = EventType.Fate,
                Name = "Serving Darkness",
                demiatma = Demiatma.CaputMortuum,
                monster = Monster.Lifereaper,
                start = new Vector3(413.00f, 96.00f, -13.00f),
            }
        },
        {
            1976,
            new EventData
            {
                id = 1976,
                type = EventType.Fate,
                Name = "Persistent Pots",
                demiatma = Demiatma.Orpiment,
                notes = MonsterNote.PersistentPots,
                start = new Vector3(200.00f, 111.73f, -215.00f),
            }
        },
        {
            1977,
            new EventData
            {
                id = 1977,
                type = EventType.Fate,
                Name = "Pleading Pots",
                demiatma = Demiatma.Verdigris,
                notes = MonsterNote.PersistentPots,
                start = new Vector3(-481.00f, 75.00f, 528.00f),
            }
        },
    };

    public readonly static Dictionary<uint, EventData> CriticalEncounters = new()
    {
        {
            48,
            new EventData
            {
                id = 48,
                type = EventType.CriticalEncounter,
                Name = "The Forked Tower: Blood",
            }
        },
        {
            33,
            new EventData
            {
                id = 33,
                type = EventType.CriticalEncounter,
                Name = "Scourge of the Mind",
                demiatma = Demiatma.Azurite,
                monster = Monster.MysteriousMindflayer,
                aethernet = Aethernet.Eldergrowth,
            }
        },
        {
            34,
            new EventData
            {
                id = 34,
                type = EventType.CriticalEncounter,
                Name = "The Black Regiment",
                demiatma = Demiatma.Orpiment,
                soulshard = SoulShard.Ranger,
                monster = Monster.BlackStar,
                notes = MonsterNote.BlackChocobos,
                aethernet = Aethernet.Eldergrowth,
            }
        },
        {
            35,
            new EventData
            {
                id = 35,
                type = EventType.CriticalEncounter,
                Name = "The Unbridled",
                demiatma = Demiatma.Azurite,
                soulshard = SoulShard.Berserker,
                monster = Monster.CrescentBerserker,
                notes = MonsterNote.CrescentBerserker,
                aethernet = Aethernet.Eldergrowth,
            }
        },
        {
            36,
            new EventData
            {
                id = 36,
                type = EventType.CriticalEncounter,
                Name = "Crawling Death",
                demiatma = Demiatma.Azurite,
                monster = Monster.DeathClawOccultCrescent,
                aethernet = Aethernet.Eldergrowth,
            }
        },
        {
            37,
            new EventData
            {
                id = 37,
                type = EventType.CriticalEncounter,
                Name = "Calamity Bound",
                demiatma = Demiatma.Verdigris,
                monster = Monster.CloisterDemon,
                notes = MonsterNote.CloisterDemon,
                aethernet = Aethernet.Stonemarsh,
            }
        },
        {
            38,
            new EventData
            {
                id = 38,
                type = EventType.CriticalEncounter,
                Name = "Trial by Claw",
                demiatma = Demiatma.Malachite,
                monster = Monster.CrystalDragon,
                aethernet = Aethernet.CrystallizedCaverns,
            }
        },
        {
            39,
            new EventData
            {
                id = 39,
                type = EventType.CriticalEncounter,
                Name = "From Times Bygone",
                demiatma = Demiatma.Malachite,
                monster = Monster.MythicIdol,
                notes = MonsterNote.MythicIdol,
                aethernet = Aethernet.Stonemarsh,
            }
        },
        {
            40,
            new EventData
            {
                id = 40,
                type = EventType.CriticalEncounter,
                Name = "Company of Stone",
                demiatma = Demiatma.CaputMortuum,
                monster = Monster.OccultKnight,
                aethernet = Aethernet.BaseCamp,
            }
        },
        {
            41,
            new EventData
            {
                id = 41,
                type = EventType.CriticalEncounter,
                Name = "Shark Attack",
                demiatma = Demiatma.Realgar,
                monster = Monster.NymianPotaladus,
                notes = MonsterNote.NymianPotaladus,
                aethernet = Aethernet.TheWanderersHaven,
            }
        },
        {
            42,
            new EventData
            {
                id = 42,
                type = EventType.CriticalEncounter,
                Name = "On the Hunt",
                demiatma = Demiatma.CaputMortuum,
                soulshard = SoulShard.Oracle,
                monster = Monster.LionRampant,
                aethernet = Aethernet.Eldergrowth,
            }
        },
        {
            43,
            new EventData
            {
                id = 43,
                type = EventType.CriticalEncounter,
                Name = "With Extreme Prejudice",
                demiatma = Demiatma.Realgar,
                monster = Monster.CommandUrn,
                aethernet = Aethernet.TheWanderersHaven,
            }
        },
        {
            44,
            new EventData
            {
                id = 44,
                type = EventType.CriticalEncounter,
                Name = "Noise Complaint",
                demiatma = Demiatma.Orpiment,
                monster = Monster.NeoGarula,
                aethernet = Aethernet.BaseCamp,
            }
        },
        {
            45,
            new EventData
            {
                id = 45,
                type = EventType.CriticalEncounter,
                Name = "Cursed Concern",
                demiatma = Demiatma.Realgar,
                monster = Monster.TradeTortoise,
                notes = MonsterNote.TradeTortoise,
                aethernet = Aethernet.TheWanderersHaven,
            }
        },
        {
            46,
            new EventData
            {
                id = 46,
                type = EventType.CriticalEncounter,
                Name = "Eternal Watch",
                demiatma = Demiatma.CaputMortuum,
                monster = Monster.RepairedLion,
                aethernet = Aethernet.Eldergrowth,
            }
        },
        {
            47,
            new EventData
            {
                id = 47,
                type = EventType.CriticalEncounter,
                Name = "Flame of Dusk",
                demiatma = Demiatma.Malachite,
                monster = Monster.Hinkypunk,
                aethernet = Aethernet.CrystallizedCaverns,
            }
        },
    };
}
