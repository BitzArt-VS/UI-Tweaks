using Vintagestory.API.Common;

namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Mouse event payload delivered to interactive component handlers.
/// <para>
/// <see cref="Position"/> is in <b>logical (unscaled) pixels</b>, relative to the dialog's
/// render surface — the same coordinate space the layout pass operates in.
/// </para>
/// <para>
/// <see cref="AbsolutePosition"/> is in <b>logical (unscaled) pixels</b> from the top-left
/// corner of the screen. Use it when computing deltas across drag events that move the
/// dialog — the dialog-local reference frame shifts with each move, but the screen-absolute
/// frame does not.
/// </para>
/// </summary>
public readonly record struct GuiMouseEventArgs(
    GuiPoint Position,
    GuiPoint AbsolutePosition,
    EnumMouseButton Button)
{
    /// <summary>
    /// Non-zero only for wheel events; zero for all click/move events.
    /// </summary>
    public float WheelDelta { get; init; } = 0f;
}
