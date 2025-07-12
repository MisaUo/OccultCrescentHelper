using BOCCHI.Data;
using Ocelot;

namespace BOCCHI.Modules.ForkedTower;

public class Panel
{
    public void Draw(ForkedTowerModule module)
    {
        if (!ZoneData.IsInForkedTower())
        {
            // return;
        }

        OcelotUI.Title("Forked Tower:");
        OcelotUI.Indent(() => { OcelotUI.LabelledValue("Tower ID", module.TowerHash); });
    }
}
