using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.DalamudServices;

namespace BOCCHI.Memory;

public class InitZone : IDisposable
{
    public delegate nint InitZoneDelegate(nint a1, int a2, nint a3);

    private const string Signature = "E8 ?? ?? ?? ?? 45 33 C0 48 8D 56 ?? 8B CF E8 ?? ?? ?? ?? 48 8D 4E";

    [Signature(Signature, DetourName = nameof(Callback))]
    private readonly Hook<InitZoneDelegate> _Hook = null!;

    public InitZone()
    {
        Svc.Hook.InitializeFromAttributes(this);
    }

    public void Dispose()
    {
        if (_Hook?.IsEnabled == true) _Hook?.Disable();

        if (_Hook?.IsDisposed == false) _Hook?.Dispose();
    }

    public event InitZoneDelegate? OnInitZone;

    private nint Callback(nint a1, int a2, nint a3)
    {
        OnInitZone?.Invoke(a1, a2, a3);
        return _Hook.Original(a1, a2, a3);
    }

    public void Enable()
    {
        _Hook?.Enable();
    }

    public void Disable()
    {
        _Hook?.Disable();
    }
}
