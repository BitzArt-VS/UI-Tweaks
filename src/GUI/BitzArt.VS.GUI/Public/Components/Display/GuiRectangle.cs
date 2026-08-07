using Cairo;

namespace BitzArt.VS.GUI;

/// <summary>
/// A leaf component that fills its bounds with a solid <see cref="Color"/>.
/// Both axes fit content by default; set explicit width and height at the call site
/// to produce a fixed or fractional filled rectangle.
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
