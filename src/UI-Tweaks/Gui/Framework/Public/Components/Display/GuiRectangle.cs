using Cairo;

namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// A leaf component that fills its bounds with a solid <see cref="Color"/>.
/// Both axes default to <see cref="GuiSizeMode.FitContent"/> — override via fluent
/// extensions or set explicit <c>width</c>/<c>height</c> at the call site to produce a
/// fixed-size filled rectangle.
/// </summary>
public class GuiRectangle : GuiComponent
{
    /// <summary>Fill colour. Defaults to <see cref="GuiColor.Transparent"/> — a no-op draw.</summary>
    public GuiColor Color { get; set; }

    public override void Render(Context context, GuiBounds bounds)
    {
        if (Color.A <= 0)
        {
            return;
        }

        var position = bounds.Position!.Value;
        var size = bounds.Size!.Value;
        double width = size.Width!.Value;
        double height = size.Height!.Value;

        context.SetSourceRGBA(Color.R, Color.G, Color.B, Color.A);
        context.Rectangle(position.X, position.Y, width, height);
        context.Fill();
    }
}
