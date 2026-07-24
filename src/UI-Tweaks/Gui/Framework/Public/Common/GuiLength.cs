using System.Diagnostics;
using System.Globalization;

namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Declares a one-dimensional fixed or fractional length. A declaration may remain
/// relative until <see cref="Resolve"/> converts it into logical-pixel geometry.
/// </summary>
public readonly struct GuiLength
{
    private readonly Kind _kind;
    private readonly double _fixedValue;
    private readonly double _fractionalValue;

    private GuiLength(
        Kind kind,
        double fixedValue = 0,
        double fractionalValue = 0)
    {
        _kind = kind;
        _fixedValue = fixedValue;
        _fractionalValue = fractionalValue;
    }

    public static GuiLength Fixed(double value)
        => new(Kind.Fixed, fixedValue: value);

    public static GuiLength Fraction(double value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Fractional length must be non-negative.");
        }

        return new GuiLength(
            Kind.Fraction,
            fractionalValue: value);
    }

    public static GuiLength Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        string trimmed = value.Trim();
        if (trimmed.EndsWith('%'))
        {
            string percent = trimmed[..^1].Trim();
            return Fraction(double.Parse(percent, CultureInfo.InvariantCulture) / 100.0);
        }

        return Fixed(double.Parse(trimmed, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Resolves this declaration to a logical-pixel value, or <c>null</c> when
    /// a fractional declaration has no available length to resolve against.
    /// </summary>
    /// <param name="availableLength">
    /// The available logical-pixel length, or <c>null</c> when the axis is unlimited.
    /// </param>
    public double? Resolve(double? availableLength)
    {
        return _kind switch
        {
            Kind.Fixed => _fixedValue,
            Kind.Fraction => availableLength * _fractionalValue,
            _ => throw new UnreachableException(),
        };
    }

    public static implicit operator GuiLength(int value) => Fixed(value);
    public static implicit operator GuiLength(double value) => Fixed(value);
    public static implicit operator GuiLength(string value) => Parse(value);

    private enum Kind
    {
        Fixed,
        Fraction,
    }
}
