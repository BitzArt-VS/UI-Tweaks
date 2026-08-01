using System.Diagnostics;
using System.Globalization;

namespace BitzArt.VS.GUI;

/// <summary>
/// Defines a component's size in one direction.
/// </summary>
public readonly struct GuiLengthRule
{
    private readonly Kind _kind;
    private readonly double _fixedValue;
    private readonly double _fractionalValue;

    private GuiLengthRule(
        Kind kind,
        double fixedValue = 0,
        double fractionalValue = 0)
    {
        _kind = kind;
        _fixedValue = fixedValue;
        _fractionalValue = fractionalValue;
    }

    /// <summary>
    /// Specifies a fixed size in logical pixels.
    /// </summary>
    public static GuiLengthRule Fixed(double value)
        => new(Kind.Fixed, fixedValue: value);

    /// <summary>
    /// Fills the available space.
    /// </summary>
    public static GuiLengthRule Fill
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
    public static GuiLengthRule Fraction(double value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Fractional length must be non-negative.");
        }

        return new GuiLengthRule(
            Kind.Fraction,
            fractionalValue: value);
    }

    /// <summary>
    /// Parses a fixed logical-pixel size or a percentage of available space.
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
    public static GuiLengthRule Parse(string value)
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
    /// Available length in logical pixels, or <see langword="null"/> if it is unknown.
    /// </param>
    /// <returns>
    /// Resulting size in logical pixels, or <see langword="null"/> if it cannot be
    /// determined without an available length.
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
    /// Space available to the component in logical pixels.
    /// </param>
    /// <param name="fixedRule">
    /// Fixed size specified by the component, or <see langword="null"/> if none.
    /// </param>
    /// <param name="minimumRule">
    /// Minimum size specified by the component, or <see langword="null"/> if none.
    /// </param>
    /// <param name="maximumRule">
    /// Maximum size specified by the component, or <see langword="null"/> if none.
    /// </param>
    /// <returns>
    /// Resolved component size in logical pixels, or <see langword="null"/> if it cannot
    /// be determined.
    /// </returns>
    public static double? Resolve(
        double? available,
        GuiLengthRule? fixedRule,
        GuiLengthRule? minimumRule,
        GuiLengthRule? maximumRule)
        => Resolve(
            available,
            candidate: available,
            fixedRule,
            minimumRule,
            maximumRule);

    /// <inheritdoc cref="Resolve(double?, GuiLengthRule?, GuiLengthRule?, GuiLengthRule?)"/>
    /// <param name="candidate">
    /// Size proposed by the layout in logical pixels, or <see langword="null"/> if unknown.
    /// </param>
    public static double? Resolve(double? available, double? candidate, GuiLengthRule? fixedRule, GuiLengthRule? minimumRule, GuiLengthRule? maximumRule)
    {
        var value = fixedRule is not null
            ? fixedRule.Value.Resolve(available)
            : candidate;

        var minimum = minimumRule?.Resolve(available);
        var maximum = maximumRule?.Resolve(available);

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
    /// Converts an integer to a fixed size in logical pixels.
    /// </summary>
    public static implicit operator GuiLengthRule(int value) => Fixed(value);
    /// <summary>
    /// Converts a <see langword="float"/> value to a fixed size in logical pixels.
    /// </summary>
    public static implicit operator GuiLengthRule(float value) => Fixed(value);
    /// <summary>
    /// Converts a <see langword="double"/> value to a fixed size in logical pixels.
    /// </summary>
    public static implicit operator GuiLengthRule(double value) => Fixed(value);
    /// <inheritdoc cref="Parse(string)"/>
    public static implicit operator GuiLengthRule(string value) => Parse(value);

    private enum Kind
    {
        Fixed,
        Fraction,
    }
}
