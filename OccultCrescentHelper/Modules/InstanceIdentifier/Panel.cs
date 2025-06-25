using Ocelot;

namespace BOCCHI.Modules.InstanceIdentifier;

public class Panel
{
    public void Draw(InstanceIdentifierModule module)
    {
        OcelotUI.LabelledValue(module.T("panel.id.label"), module.instance);
    }
}
