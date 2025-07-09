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
            if (ImGui.Button(module.farmer.Running ? I18N.T("generic.label.stop") : I18N.T("generic.label.start")))
            {
                module.farmer.Toggle();
            }

            if (module.farmer.Running)
            {
                OcelotUI.LabelledValue("Phase", module.farmer.Phase);
            }

            OcelotUI.LabelledValue("Not Engaged", module.farmer.NotInCombat.Count());
            OcelotUI.LabelledValue("Engaged", module.farmer.InCombat.Count());
        });
    }
}
