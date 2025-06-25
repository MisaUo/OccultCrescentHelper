using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ImGuiNET;

namespace OccultCrescentHelper;

public static class Helpers
{
    public static unsafe void DrawLine(Vector3 start, Vector3 end, float thickness, Vector4 color)
    {
        var camera = CameraManager.Instance()->CurrentCamera;
        var renderCamera = camera->RenderCamera;

        Matrix4x4 view = camera->ViewMatrix;
        Matrix4x4 projection = renderCamera->ProjectionMatrix;
        var nearPlane = renderCamera->NearPlane;
        var width = Device.Instance()->Width;
        var height = Device.Instance()->Height;

        if (!CameraHelper.WorldLineToScreen(start, end, view, projection, nearPlane, width, height, out var screenStart, out var screenEnd))
            return;

        var imguiColor = ImGui.ColorConvertFloat4ToU32(color);
        ImGui.GetBackgroundDrawList().AddLine(screenStart, screenEnd, imguiColor, thickness);
    }
}
