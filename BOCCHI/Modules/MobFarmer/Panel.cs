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
            if (ImGui.Button(module.Farmer.Running ? I18N.T("generic.label.stop") : I18N.T("generic.label.start")))
            {
                module.Farmer.Toggle(module);
            }

            if (module.Farmer.Running)
            {
                OcelotUI.LabelledValue("Phase", module.Farmer.StateMachine.State);
            }

            OcelotUI.LabelledValue("Not Engaged", module.Scanner.NotInCombat.Count());
            OcelotUI.LabelledValue("Engaged", module.Scanner.InCombat.Count());
        });
    }
}
