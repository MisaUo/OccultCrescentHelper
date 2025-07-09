﻿using FFXIVClientStructs.FFXIV.Client.Game;

namespace BOCCHI.ActionHelpers;

public static partial class Actions
{
    public static class Freelancer
    {
        public static Action Resuscitation { get; private set; } = new(ActionType.GeneralAction, 31);

        public static Action Treasuresight { get; private set; } = new(ActionType.GeneralAction, 32);
    }
}
