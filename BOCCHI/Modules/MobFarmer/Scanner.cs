using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;

namespace BOCCHI.Modules.MobFarmer;

public class Scanner
{
    private const int FOPPER = 17851;

    public List<IGameObject> Foppers { get; private set; } = [];

    public void Tick(IFramework _)
    {
        Foppers = Svc.Objects.Where(o => o.DataId == FOPPER && !o.IsDead && o.IsTargetable).OrderBy(Player.DistanceTo).ToList();
    }
}
