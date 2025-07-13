namespace BOCCHI.Modules.Debug.Panels;

public abstract class Panel
{
    public abstract string GetName();

    public virtual void Update(DebugModule module)
    {
    }

    public virtual void Render(DebugModule module)
    {
    }

    public virtual void OnTerritoryChanged(ushort id, DebugModule module)
    {
    }
}
