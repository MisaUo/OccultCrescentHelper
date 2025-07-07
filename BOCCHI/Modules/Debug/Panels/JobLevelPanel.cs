using System;
using System.Collections.Generic;
using System.Linq;
using BOCCHI.Enums;
using BOCCHI.Modules.Automator;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using ImGuiNET;
using Lumina.Excel.Sheets;
using Ocelot;

namespace BOCCHI.Modules.Debug.Panels;

public class JobLevelPanel : Panel
{
    private List<IGameObject> enemies = [];

    public override string GetName()
    {
        return "Job Level";
    }

    public override unsafe void Draw(DebugModule module)
    {
        // var level = PublicContentOccultCrescent.GetState()->SupportJobLevels[1];
        var state = PublicContentOccultCrescent.GetState();
        OcelotUI.Indent(() =>
        {
            foreach (var job in Svc.Data.GetExcelSheet<MKDSupportJob>())
            {
                OcelotUI.Title(job.Unknown0.ToString());
                OcelotUI.Indent(() =>
                {
                    var level = state->SupportJobLevels[(byte)job.RowId];
                    OcelotUI.LabelledValue("Level", $"{level}/{job.Unknown10}");
                });

                OcelotUI.Indent(() =>
                {
                    var exp = state->SupportJobExperience[(byte)job.RowId];
                    OcelotUI.LabelledValue("Exp", exp);
                });
            }
        });
    }
}
