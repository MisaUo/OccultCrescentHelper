using System.Linq;
using ImGuiNET;
using Ocelot;

namespace BOCCHI.Modules.MobFarmer;

public class Panel
{
    public void Draw(MobFarmerModule module)
    {
        OcelotUI.Title("Mob Farmer:");
        OcelotUI.Indent(() =>
        {
            if (ImGui.Button(module.Running ? I18N.T("generic.label.stop") : I18N.T("generic.label.start")))
            {
                module.Toggle();
            }

            if (module.Running)
            {
                OcelotUI.LabelledValue("Phase", module.phase);
                OcelotUI.LabelledValue("Not Engaged", module.NotInCombat.Count());
                OcelotUI.LabelledValue("Engaged", module.InCombat.Count());
            }
        });
    }
}
