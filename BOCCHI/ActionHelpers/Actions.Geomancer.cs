using FFXIVClientStructs.FFXIV.Client.Game;

namespace BOCCHI.ActionHelpers;

public static partial class Actions
{
    public static class Geomancer
    {
        public static Action BattleBell { get; private set; } = new(ActionType.GeneralAction, 31);
        public static Action RingingRespite { get; private set; } = new(ActionType.GeneralAction, 33);
    }
}
