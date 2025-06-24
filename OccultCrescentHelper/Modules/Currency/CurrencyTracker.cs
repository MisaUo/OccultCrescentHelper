using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace BOCCHI.Modules.Currency;

public class CurrencyTracker
{
    private float gainedGold;

    private float gainedSilver;

    private DateTime goldStartTime = DateTime.UtcNow;

    private float lastGold;

    private float lastSilver;

    private DateTime silverStartTime = DateTime.UtcNow;

    public CurrencyTracker()
    {
        Reset();
    }

    public void Tick(IFramework _)
    {
        var currentGold = GetGold();
        var currentSilver = GetSilver();

        var goldDelta = currentGold - lastGold;
        var silverDelta = currentSilver - lastSilver;

        if (goldDelta > 0)
            gainedGold += goldDelta;

        if (silverDelta > 0)
            gainedSilver += silverDelta;

        lastGold = currentGold;
        lastSilver = currentSilver;
    }

    public void TerritoryChanged(ushort _)
    {
        Reset();
    }

    public void ResetSilver()
    {
        lastSilver = GetSilver();
        gainedSilver = 0;
        silverStartTime = DateTime.UtcNow;
    }

    public void ResetGold()
    {
        lastGold = GetGold();
        gainedGold = 0;
        goldStartTime = DateTime.UtcNow;
    }

    public void Reset()
    {
        ResetSilver();
        ResetGold();
    }

    public float GetGoldPerHour()
    {
        var elapsed = (float)(DateTime.UtcNow - goldStartTime).TotalHours;
        if (elapsed <= 0)
            return 0;

        return gainedGold / elapsed;
    }

    public float GetSilverPerHour()
    {
        var elapsed = (float)(DateTime.UtcNow - silverStartTime).TotalHours;
        if (elapsed <= 0)
            return 0;

        return gainedSilver / elapsed;
    }

    private unsafe float GetGold()
    {
        return InventoryManager.Instance()->GetInventoryItemCount((uint)Currency.Gold);
    }

    private unsafe float GetSilver()
    {
        return InventoryManager.Instance()->GetInventoryItemCount((uint)Currency.Silver);
    }

    private enum Currency
    {
        Silver = 45043,
        Gold = 45044
    }
}
