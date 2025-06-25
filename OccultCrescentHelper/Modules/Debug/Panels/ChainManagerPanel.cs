using ImGuiNET;
using Ocelot;
using Ocelot.Chain;

namespace BOCCHI.Modules.Debug.Panels;

public class ChainManagerPanel : Panel
{
    public override string GetName()
    {
        return "Chain Manager";
    }

    public override void Draw(DebugModule module)
    {
        OcelotUI.Title("Chain Manager:");
        OcelotUI.Indent(() =>
        {
            var instances = ChainManager.Active();
            OcelotUI.Title("# of instances:");
            ImGui.SameLine();
            ImGui.TextUnformatted(instances.Count.ToString());

            foreach (var pair in instances)
            {
                if (pair.Value.CurrentChain == null)
                {
                    continue;
                }

                OcelotUI.Title($"{pair.Key}:");
                OcelotUI.Indent(() =>
                {
                    var current = pair.Value.CurrentChain!;
                    OcelotUI.Title("Current Chain:");
                    ImGui.SameLine();
                    ImGui.TextUnformatted(current.name);

                    OcelotUI.Title("Progress:");
                    ImGui.SameLine();
                    ImGui.TextUnformatted($"{current.progress * 100}%");

                    OcelotUI.Title("Queued Chains:");
                    ImGui.SameLine();
                    ImGui.TextUnformatted(pair.Value.QueueCount.ToString());
                });
            }
        });
    }
}
