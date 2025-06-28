namespace BOCCHI.Modules.Debug.Panels;

public abstract class Panel
{
    public abstract string GetName();

    public virtual void Tick(DebugModule module)
    {
    }

    public virtual void Draw(DebugModule module)
    {
    }

    public virtual void OnTerritoryChanged(ushort id, DebugModule module)
    {
    }
}
