using System.Linq;
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
                var shards = ZoneHelper.GetNearbyAethernetShards();
                if (shards.Count > 0)
                {
                    OcelotUI.Title("Nearby Aethernet Shards:");
                    OcelotUI.Indent(() =>
                    {
                        foreach (var shard in ZoneHelper.GetNearbyAethernetShards())
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
