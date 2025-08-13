using ImGuiNET;
using Ocelot;
using System.Linq;

namespace BOCCHI.Modules.MobFarmer;

public class Panel
{
    public void Draw(MobFarmerModule module)
    {
        OcelotUI.Title("刷怪:");
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
