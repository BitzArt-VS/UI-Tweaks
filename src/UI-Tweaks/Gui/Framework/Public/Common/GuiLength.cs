using System.Globalization;

namespace BitzArt.UI.Tweaks.Gui;

public readonly struct GuiLength
{
    private readonly GuiLengthKind _kind;

    public double Value { get; }
    public double? Minimum { get; }
    public double? Maximum { get; }

    public bool IsAuto => _kind == GuiLengthKind.Auto;
    public bool IsFixed => _kind == GuiLengthKind.Fixed;
    public bool IsFraction => _kind == GuiLengthKind.Fraction;

    private GuiLength(GuiLengthKind kind, double value, double? minimum = null, double? maximum = null)
    {
        _kind = kind;
        Value = value;
        Minimum = minimum;
        Maximum = maximum;
    }

    public static GuiLength Auto => default;
    public static GuiLength Fixed(double value) => new(GuiLengthKind.Fixed, value);

    public static GuiLength Fraction(double value, double? minimum = null, double? maximum = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Fractional size must be non-negative.");
        }

        return new GuiLength(GuiLengthKind.Fraction, value, minimum, maximum);
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

    public double Resolve(double availableSize)
    {
        double resolved = _kind switch
        {
            GuiLengthKind.Fixed => Value,
            GuiLengthKind.Fraction => availableSize * Value,
            _ => throw new InvalidOperationException("Auto sizes cannot be resolved directly."),
        };

        if (Minimum is not null)
        {
            resolved = Math.Max(Minimum.Value, resolved);
        }

        if (Maximum is not null)
        {
            resolved = Math.Min(Maximum.Value, resolved);
        }

        return resolved;
    }

    public double FixedOrDefault(double defaultValue)
        => IsFixed ? Value : defaultValue;

    internal bool CanResolve(double availableSize)
        => !IsAuto && (!IsFraction || !double.IsPositiveInfinity(availableSize));

    public static implicit operator GuiLength(int value) => Fixed(value);
    public static implicit operator GuiLength(double value) => Fixed(value);
    public static implicit operator GuiLength(string value) => Parse(value);
}

internal enum GuiLengthKind
{
    Auto,
    Fixed,
    Fraction,
}
