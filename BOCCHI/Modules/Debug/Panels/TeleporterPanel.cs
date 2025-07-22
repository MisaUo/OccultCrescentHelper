using System.Linq;
using BOCCHI.Data;
using BOCCHI.Enums;
using BOCCHI.Modules.Teleporter;
using ImGuiNET;
using Ocelot;

namespace BOCCHI.Modules.Debug.Panels;

public class TeleporterPanel : Panel
{
    public override string GetName()
    {
        return "Teleporter";
    }

    public override void Render(DebugModule module)
    {
        if (module.TryGetModule<TeleporterModule>(out var teleporter) && teleporter!.IsReady())
        {
            OcelotUI.Title("Teleporter:");
            OcelotUI.Indent(() =>
            {
                var shards = ZoneData.GetNearbyAethernetShards();
                if (shards.Count > 0)
                {
                    OcelotUI.Title("Nearby Aethernet Shards:");
                    OcelotUI.Indent(() =>
                    {
                        foreach (var shard in ZoneData.GetNearbyAethernetShards())
                        {
                            var data = AethernetData.All().First(o => o.DataId == shard.DataId);
                            ImGui.TextUnformatted(data.Aethernet.ToFriendlyString());
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
