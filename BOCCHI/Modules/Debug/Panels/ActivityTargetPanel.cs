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
using ImGuiNET;
using Lumina.Excel.Sheets;
using Ocelot;

namespace BOCCHI.Modules.Debug.Panels;

public class ActivityTargetPanel : Panel
{
    private List<IGameObject> enemies = [];

    public override string GetName()
    {
        return "Activity Targets";
    }

    public override unsafe void Draw(DebugModule module)
    {
        OcelotUI.Indent(() =>
        {
            if (EzThrottler.Throttle("ActivityTargetPanel", 1000))
            {
                enemies = GetEnemies();
            }

            foreach (var enemy in enemies)
            {
                ImGui.TextUnformatted(enemy.Name.ToString());
                OcelotUI.Indent(() =>
                {
                    OcelotUI.LabelledValue("Object Kind", enemy.ObjectKind);
                    OcelotUI.LabelledValue("Targetable", enemy.IsTargetable ? "Yes" : "No");
                    OcelotUI.LabelledValue("Is Alive", enemy.IsDead ? "No" : "Yes");
                    OcelotUI.LabelledValue("Is Activity Target", IsActivityTarget(enemy, module) ? "Yes" : "No");
                });
            }
        });
    }

    private List<IGameObject> GetEnemies()
    {
        return Svc.Objects
            .Where(o => o != null && Player.DistanceTo(o) <= 50f && o.ObjectKind == ObjectKind.BattleNpc)
            .OrderBy(o => o.IsTargetable ? 0 : 1)
            .ToList();
    }

    private unsafe bool IsActivityTarget(IGameObject? obj, DebugModule module)
    {
        if (obj == null)
        {
            return false;
        }

        try
        {
            var battleChara = (BattleChara*)obj.Address;

            var id = battleChara->EventId.EntryId;
            var count = Svc.Data.GetExcelSheet<DynamicEvent>().Count();

            var activity = module.GetModule<AutomatorModule>().automator.activity;
            if (activity != null)
            {
                var data = activity.data;
                if (data.type == EventType.Fate)
                {
                    return battleChara->FateId == data!.id;
                }
            }


            var isRelatedToCurrentEvent = battleChara->EventId.EntryId == Player.BattleChara->EventId.EntryId;

            return obj.SubKind == (byte)BattleNpcSubKind.Enemy && isRelatedToCurrentEvent;
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex.Message);
            return false;
        }
    }
}
