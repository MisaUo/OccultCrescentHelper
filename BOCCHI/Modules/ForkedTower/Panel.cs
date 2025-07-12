using Ocelot;

namespace BOCCHI.Modules.ForkedTower;

public class Panel
{
    public void Draw(ForkedTowerModule module)
    {
        OcelotUI.Title("ForkedTower:");
        OcelotUI.Indent(() => {
            // Content here
        });
    }
}
