using Vintagestory.API.Common;

namespace BitzArt.VS.GUI;

public readonly record struct GuiMouseEventArgs(
    GuiPoint Position,
    GuiPoint AbsolutePosition,
    EnumMouseButton Button)
{
    public float WheelDelta { get; init; } = 0f;
}
