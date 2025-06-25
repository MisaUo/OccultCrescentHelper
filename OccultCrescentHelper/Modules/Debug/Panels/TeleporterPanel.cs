using System.Linq;
using ImGuiNET;
using OccultCrescentHelper.Enums;
using OccultCrescentHelper.Modules.Teleporter;
using Ocelot;

namespace OccultCrescentHelper.Modules.Debug.Panels;

public class TeleporterPanel : Panel
{
    public override string GetName()
    {
        return "Teleporter";
    }

    public override void Draw(DebugModule module)
    {
        if (module.TryGetModule<TeleporterModule>(out var teleporter) && teleporter!.IsReady())
        {
            OcelotUI.Title("Teleporter:");
            OcelotUI.Indent(() => {
                var shards = teleporter.teleporter.GetNearbyAethernetShards();
                if (shards.Count() > 0)
                {
                    OcelotUI.Title("Nearby Aethernet Shards:");
                    OcelotUI.Indent(() => {
                        foreach (var shard in teleporter.teleporter.GetNearbyAethernetShards())
                        {
                            var data = AethernetData.All().First(o => o.dataId == shard.DataId);
                            ImGui.TextUnformatted(data.aethernet.ToFriendlyString());
                        }
                    });
                }

                if (ImGui.Button("Test Return"))
                {
                    teleporter.teleporter.Return();
                }
            });
        }
    }
}
