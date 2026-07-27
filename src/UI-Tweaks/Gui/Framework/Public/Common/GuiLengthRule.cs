using System.Diagnostics;
using System.Globalization;

namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Declares a one-dimensional fixed or fractional length. A declaration may remain
/// relative until <see cref="Resolve"/> converts it into logical-pixel geometry.
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

    public static GuiLengthRule Fixed(double value)
        => new(Kind.Fixed, fixedValue: value);

    /// <summary>
    /// A fractional rule that consumes all available length.
    /// </summary>
    public static GuiLengthRule Fill
        => Fraction(1);

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
    /// Resolves this declaration to a logical-pixel value, or <see langword="null"/>
    /// when a fractional rule has no available length.
    /// </summary>
    /// <param name="availableLength">
    /// Available logical-pixel length, or <see langword="null"/> when unavailable.
    /// Fixed rules do not require a value.
    /// </param>
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
    /// Resolves a provisional length using available space as the initial candidate.
    /// </summary>
    /// <returns>
    /// Constrained logical-pixel length, or <see langword="null"/> when the length
    /// remains unlimited.
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

    /// <summary>
    /// Resolves a candidate length, using available space as the basis for
    /// fractional fixed, minimum, and maximum rules.
    /// </summary>
    /// <returns>
    /// Constrained logical-pixel length, or <see langword="null"/> when the length
    /// remains unlimited.
    /// </returns>
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

    public static implicit operator GuiLengthRule(int value) => Fixed(value);
    public static implicit operator GuiLengthRule(float value) => Fixed(value);
    public static implicit operator GuiLengthRule(double value) => Fixed(value);
    public static implicit operator GuiLengthRule(string value) => Parse(value);

    private enum Kind
    {
        Fixed,
        Fraction,
    }
}
