using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ImGuiNET;
using Ocelot;

namespace OccultCrescentHelper.Modules.Debug.Panels;

public class TargetPanel : Panel
{
    public override string GetName()
    {
        return "Target";
    }

    public override unsafe void Draw(DebugModule module)
    {
        OcelotUI.Indent(() =>
        {
            var target = Svc.Targets.Target;
            if (target == null)
            {
                ImGui.TextUnformatted("No target selected.");
                return;
            }

            // Try to cast to internal GameObject
            var obj = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)target.Address;

            if (obj == null)
            {
                ImGui.TextUnformatted("Target is not a native GameObject.");
                return;
            }

            void Draw<T>(string label, T value)
            {
                OcelotUI.Title($"{label}:");
                ImGui.SameLine();
                ImGui.TextUnformatted(value?.ToString() ?? "null");
            }

            Draw("Name", obj->NameString);
            Draw("EventState", obj->EventState);
            Draw("EntityId", obj->EntityId);
            Draw("LayoutId", obj->LayoutId);
            Draw("BaseId", obj->BaseId);
            Draw("OwnerId", obj->OwnerId);
            Draw("ObjectIndex", obj->ObjectIndex);
            Draw("ObjectKind", obj->ObjectKind);
            Draw("SubKind", obj->SubKind);
            Draw("Sex", obj->Sex);
            Draw("YalmDistX", obj->YalmDistanceFromPlayerX);
            Draw("TargetStatus", obj->TargetStatus);
            Draw("YalmDistZ", obj->YalmDistanceFromPlayerZ);
            Draw("TargetableStatus", obj->TargetableStatus);
            Draw("Position", obj->Position);
            Draw("Rotation", obj->Rotation);
            Draw("Scale", obj->Scale);
            Draw("Height", obj->Height);
            Draw("VfxScale", obj->VfxScale);
            Draw("HitboxRadius", obj->HitboxRadius);
            Draw("DrawOffset", obj->DrawOffset);
            Draw("EventId", obj->EventId);
            Draw("FateId", obj->FateId);
            Draw("NamePlateIconId", obj->NamePlateIconId);
            Draw("RenderFlags", obj->RenderFlags);

            // Pointers and advanced types
            Draw("DrawObject", (ulong)obj->DrawObject);
            Draw("SharedGroupLayoutInstance", (ulong)obj->SharedGroupLayoutInstance);
            Draw("LuaActor", (ulong)obj->LuaActor);
            Draw("EventHandler", (ulong)obj->EventHandler);

            // Virtual methods (callable via vtable)
            Draw("IsTargetable()", obj->GetIsTargetable());
            Draw("Radius", obj->GetRadius());
            Draw("Height (Virtual)", obj->GetHeight());
            Draw("Sex (Virtual)", obj->GetSex());
            Draw("IsDead()", obj->IsDead());
            Draw("IsNotMounted()", obj->IsNotMounted());
            Draw("IsCharacter()", obj->IsCharacter());
        });
    }


    // public override void Tick(DebugModule module)
    // {
    //     if (EzThrottler.Throttle("enemies", 2000))
    //     {
    //         // DoThing();
    //         enemies = Svc.Objects
    //             .Where(o =>
    //                 o != null &&
    //                 o.IsHostile() &&
    //                 o.IsTargetable &&
    //                 o.Name.TextValue.Length > 0
    //             )
    //             .OrderBy(o => Vector3.Distance(o.Position, Player.Position))
    //             .ToList();
    //     }
    // }
}
