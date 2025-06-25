using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace BOCCHI.Modules.Carrots;

public class Carrot
{
    private readonly IGameObject gameObject;

    public static Vector4 color = new(0.93f, 0.57f, 0.13f, 1f);

    public Carrot(IGameObject obj)
    {
        gameObject = obj;
    }

    public bool IsValid()
    {
        return gameObject != null && !gameObject.IsDead && gameObject.IsValid();
    }

    public Vector3 GetPosition()
    {
        return gameObject.Position;
    }
}
