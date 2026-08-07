using System.Diagnostics;
using System.Globalization;

namespace BitzArt.VS;

/// <summary>
/// Defines a component's size in one direction.
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

    /// <summary>
    /// Specifies a fixed size in logical pixels before display scaling.
    /// </summary>
    public static GuiLength Fixed(double value)
        => new(Kind.Fixed, fixedValue: value);

    /// <summary>
    /// Fills the available space.
    /// </summary>
    public static GuiLength Fill
        => Fraction(1);

    /// <summary>
    /// Specifies a fraction of the available space.
    /// </summary>
    /// <param name="value">
    /// Fraction to use, where <c>1</c> fills the available space.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
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

    /// <summary>
    /// Parses a fixed size in logical pixels before display scaling, or a percentage
    /// of available space.
    /// </summary>
    /// <param name="value">Text to parse using the invariant culture.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="value"/> is not a valid number or percentage.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> specifies a negative percentage.
    /// </exception>
    public static GuiLength Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        value = value.Trim();
        if (value.EndsWith('%'))
        {
            string percent = value[..^1].Trim();
            return Fraction(double.Parse(percent, CultureInfo.InvariantCulture) / 100.0);
        }

        return Fixed(double.Parse(value, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Calculates the resulting size for the given amount of available space.
    /// </summary>
    /// <param name="availableLength">
    /// Available length in logical pixels before display scaling, or
    /// <see langword="null"/> if it is unknown.
    /// </param>
    /// <returns>
    /// Resulting size in logical pixels before display scaling, or
    /// <see langword="null"/> if it cannot be determined without an available length.
    /// </returns>
    public double? Resolve(double? availableLength)
    {
        return _kind switch
        {
            Kind.Fixed => _fixedValue,
            Kind.Fraction => availableLength is null
                ? null
                : availableLength.Value * _fractionalValue,

            _ => throw new UnreachableException(),
        };
    }

    /// <summary>
    /// Resolves a component size from its optional fixed, minimum, and maximum lengths.
    /// </summary>
    /// <param name="available">
    /// Space available to the component in logical pixels before display scaling.
    /// </param>
    /// <param name="fixedLength">
    /// Fixed size specified by the component, or <see langword="null"/> if none.
    /// </param>
    /// <param name="minimumLength">
    /// Minimum size specified by the component, or <see langword="null"/> if none.
    /// </param>
    /// <param name="maximumLength">
    /// Maximum size specified by the component, or <see langword="null"/> if none.
    /// </param>
    /// <returns>
    /// Resolved component size in logical pixels before display scaling, or
    /// <see langword="null"/> if it cannot be determined.
    /// </returns>
    public static double? Resolve(
        double? available,
        GuiLength? fixedLength,
        GuiLength? minimumLength,
        GuiLength? maximumLength)
        => Resolve(
            available,
            candidate: available,
            fixedLength,
            minimumLength,
            maximumLength);

    /// <inheritdoc cref="Resolve(double?, GuiLength?, GuiLength?, GuiLength?)"/>
    /// <param name="candidate">
    /// Size proposed by the layout in logical pixels before display scaling, or
    /// <see langword="null"/> if unknown.
    /// </param>
    public static double? Resolve(double? available, double? candidate, GuiLength? fixedLength, GuiLength? minimumLength, GuiLength? maximumLength)
    {
        var value = fixedLength is not null
            ? fixedLength.Value.Resolve(available)
            : candidate;

        var minimum = minimumLength?.Resolve(available);
        var maximum = maximumLength?.Resolve(available);

        return Clamp(value, minimum, maximum);
    }

    private static double? Clamp(double? value, double? min, double? max)
    {
        if (value is null)
        {
            return max;
        }

        if (min is not null && value < min)
        {
            return min;
        }

        if (max is not null && value > max)
        {
            return max;
        }

        return value;
    }

    /// <summary>
    /// Converts an integer to a fixed size in logical pixels before display scaling.
    /// </summary>
    public static implicit operator GuiLength(int value) => Fixed(value);
    /// <summary>
    /// Converts a <see langword="float"/> value to a fixed size in logical pixels
    /// before display scaling.
    /// </summary>
    public static implicit operator GuiLength(float value) => Fixed(value);
    /// <summary>
    /// Converts a <see langword="double"/> value to a fixed size in logical pixels
    /// before display scaling.
    /// </summary>
    public static implicit operator GuiLength(double value) => Fixed(value);
    /// <inheritdoc cref="Parse(string)"/>
    public static implicit operator GuiLength(string value) => Parse(value);

    private enum Kind
    {
        Fixed,
        Fraction,
    }
}
