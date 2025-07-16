using BOCCHI.Data;

namespace BOCCHI.Modules.ForkedTower;

public class BlankRotationPlugin : IRotationPlugin
{
    public void PhantomJobOn(Job? job = null)
    {
    }

    public void PhantomJobOff(Job? job = null)
    {
    }

    public void Dispose()
    {
    }
}
