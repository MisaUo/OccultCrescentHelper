using FFXIVClientStructs.FFXIV.Client.Game;

namespace BOCCHI.ActionHelpers;

public static class Actions
{
    // General
    public static Action Sprint { get; private set; } = new(ActionType.GeneralAction, 4);

    // OC
    public static Action BattleBell { get; private set; } = new(ActionType.GeneralAction, 31);
}
