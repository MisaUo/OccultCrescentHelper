using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using Ocelot;
using Ocelot.IPC;

namespace BOCCHI.Modules.Debug.Panels;

public class CarrotPanel : Panel
{
    private List<Vector3> Carrots = [];

    public unsafe CarrotPanel()
    {
        // var layout = LayoutWorld.Instance()->ActiveLayout;
        // if (layout == null)
        // {
        //     return;
        // }
        //
        // if (!layout->InstancesByType.TryGetValue(InstanceType.EventNpc, out var mapPtr, false))
        // {
        //     return;
        // }
        //
        // foreach (ILayoutInstance* instance in mapPtr.Value->Values)
        // {
        //     var transform = instance->GetTransformImpl();
        //     var position = transform->Translation;
        //     if (position.Y <= -10f)
        //     {
        //         continue;
        //     }
        //     
        //     Svc.Log.Debug(instance->SubId.ToString());
        //
        //     // var treasureRowId = Unsafe.Read<uint>((byte*)instance + 0x30);
        //     // var sgbId = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Treasure>().GetRow(treasureRowId).SGB.RowId;
        //     // if (sgbId != 1596 && sgbId != 1597)
        //     // {
        //     //     continue;
        //     // }
        //
        //     // Treasure.Add((treasureRowId, position, sgbId));
        // }

        // Treasure = Treasure.OrderBy(t => t.id).ToList();
    }

    public override string GetName()
    {
        return "Carrot Helper";
    }


    public override void Draw(DebugModule module)
    {
        // OcelotUI.LabelledValue("Bronze", Treasure.Count(t => t.type == 1596).ToString()); // 60
        // OcelotUI.LabelledValue("Silver", Treasure.Count(t => t.type == 1597).ToString()); // 8
        //
        // OcelotUI.Indent(() =>
        // {
        //     foreach (var data in Treasure)
        //     {
        //         OcelotUI.LabelledValue("Id", data.id.ToString());
        //
        //         OcelotUI.Indent(() =>
        //         {
        //             OcelotUI.LabelledValue("Position", $"{data.position.X:f2}, {data.position.Y:f2}, {data.position.Z:f2}");
        //             OcelotUI.LabelledValue("Type", data.type.ToString());
        //         });
        //     }
        // });
    }
}
