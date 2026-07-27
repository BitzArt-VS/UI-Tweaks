namespace BitzArt.UI.Tweaks.Gui;

public static class GuiTreeBuilderExtensions
{
    /// <summary>
    /// Configures <see cref="GuiComponentLayoutParameters"/> for this slot.
    /// </summary>
    public static TBuilder ConfigureLayout<TBuilder>(this TBuilder builder, Action<GuiComponentLayoutParameters> configure)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddLayoutConfiguration(configure);
        return builder;
    }

    public static IGuiTreeBuilder<T> Configure<T>(this IGuiTreeBuilder<T> builder, Action<T> configure)
        where T : IGuiNode
        => builder.AddConfigurationAction(configure);

    /// <summary>
    /// Declares a node slot of type <typeparamref name="T"/> at <paramref name="key"/>.
    /// </summary>
    public static IGuiTreeBuilder<T> Add<T>(
        this IGuiTreeBuilder builder,
        int key)
        where T : IGuiNode, new()
        => builder.AddComponent<T>(key);

    /// <summary>
    /// Declares a <see cref="GuiDialogBackground"/> slot at <paramref name="key"/>.
    /// Paints the vanilla shaded-dialog look (rounded fill colour overlaid with a tiled
    /// texture and an outer stroke). Override any of the texture / colour / stroke
    /// parameters to retune the recipe; pass <paramref name="content"/> to populate the
    /// inner area.
    /// </summary>
    public static IGuiTreeBuilder<GuiDialogBackground> AddDialogBackground(
        this IGuiTreeBuilder builder,
        int key,
        GuiTreeFragment? content = null)
    {
        var backgroundBuilder = builder.AddComponent<GuiDialogBackground>(key);
        return content is null
            ? backgroundBuilder
            : backgroundBuilder.Configure(background => background.Content = content);
    }

    /// <summary>
    /// Declares a <see cref="GuiDialogTitleBar"/> slot at <paramref name="key"/>.
    /// Paints the vanilla title-bar chrome (lighter rounded fill, inner highlight bevel,
    /// open three-sided dark border) with <paramref name="title"/> drawn inside.
    /// </summary>
    public static IGuiTreeBuilder<GuiDialogTitleBar> AddDialogTitleBar(
        this IGuiTreeBuilder builder,
        int key,
        string title,
        GuiFontStyle? titleFont = null,
        Action<double, double>? onDrag = null,
        Action? onClose = null)
        => AddDialogTitleBarCore(builder, key, title, titleFont, onDrag,
            onClose is null ? default : (GuiCallback)onClose);

    /// <summary>
    /// Asynchronous-handler overload of
    /// <see cref="AddDialogTitleBar(IGuiTreeBuilder, int, string, GuiFontStyle?, Action{double, double}, Action)"/>.
    /// </summary>
    public static IGuiTreeBuilder<GuiDialogTitleBar> AddDialogTitleBar(
        this IGuiTreeBuilder builder,
        int key,
        string title,
        Func<System.Threading.Tasks.Task> onClose,
        GuiFontStyle? titleFont = null,
        Action<double, double>? onDrag = null)
        => AddDialogTitleBarCore(builder, key, title, titleFont, onDrag, (GuiCallback)onClose);

    private static IGuiTreeBuilder<GuiDialogTitleBar> AddDialogTitleBarCore(
        IGuiTreeBuilder builder,
        int key,
        string title,
        GuiFontStyle? titleFont,
        Action<double, double>? onDrag,
        GuiCallback onClose)
    {
        var titleBarBuilder = builder.AddComponent<GuiDialogTitleBar>(key)
            .Configure(titleBar =>
            {
                titleBar.Title = title;
                titleBar.OnDrag = onDrag;
                titleBar.OnClose = onClose;
            });

        if (titleFont is not null)
        {
            titleBarBuilder = titleBarBuilder.Configure(titleBar => titleBar.TitleFont = titleFont.Value);
        }

        return titleBarBuilder;
    }

    /// <summary>
    /// Declares a <see cref="GuiLabel"/> slot at <paramref name="key"/> and sets its
    /// <see cref="GuiLabel.Text"/>. Optionally supply a <paramref name="font"/>; if omitted the label uses
    /// <see cref="GuiFontStyle.Default"/>.
    /// </summary>
    public static IGuiTreeBuilder<GuiLabel> AddLabel(
        this IGuiTreeBuilder builder,
        int key,
        string text,
        GuiFontStyle? font = null)
    {
        var labelBuilder = builder.AddComponent<GuiLabel>(key)
            .Configure(label => label.Text = text);
        return font is null
            ? labelBuilder
            : labelBuilder.Configure(label => label.Font = font.Value);
    }

    /// <summary>
    /// Declares a <see cref="GuiRectangle"/> slot at <paramref name="key"/> and
    /// optionally sets its <see cref="GuiRectangle.Color"/>.
    /// </summary>
    public static IGuiTreeBuilder<GuiRectangle> AddRectangle(
        this IGuiTreeBuilder builder,
        int key,
        GuiColor? color = null)
    {
        var rectangleBuilder = builder.AddComponent<GuiRectangle>(key);
        return color is null
            ? rectangleBuilder
            : rectangleBuilder.Configure(rectangle => rectangle.Color = color.Value);
    }

    /// <summary>
    /// Declares a <see cref="GuiSeparator"/> slot at <paramref name="key"/>. The
    /// separator defaults to 1 px tall, full-width, and
    /// <see cref="GuiStyle.DialogTitleBarBgColor"/>. Override any property or
    /// layout parameter via fluent <c>.Configure(...)</c> / <c>.ConfigureLayout(...)</c>.
    /// </summary>
    public static IGuiTreeBuilder<GuiSeparator> AddSeparator(
        this IGuiTreeBuilder builder,
        int key)
        => builder.AddComponent<GuiSeparator>(key);

    /// <summary>
    /// Declares a <see cref="GuiInset"/> slot at <paramref name="key"/>.
    /// Pass <paramref name="content"/> to nest a render fragment inside the inset — children
    /// are drawn between the brightness overlay and the emboss ring, producing a recessed
    /// look. Leave it null when using the inset purely as chrome over absolute-positioned
    /// content.
    /// </summary>
    public static IGuiTreeBuilder<GuiInset> AddInset(
        this IGuiTreeBuilder builder,
        int key,
        int? depth = null,
        float? brightness = null,
        double? radius = null,
        GuiTreeFragment? content = null)
    {
        var insetBuilder = builder.AddComponent<GuiInset>(key);
        if (depth is not null)
        {
            insetBuilder = insetBuilder.Configure(inset => inset.Depth = depth.Value);
        }

        if (brightness is not null)
        {
            insetBuilder = insetBuilder.Configure(inset => inset.Brightness = brightness.Value);
        }

        if (radius is not null)
        {
            insetBuilder = insetBuilder.Configure(inset => inset.Radius = radius.Value);
        }

        return content is null
            ? insetBuilder
            : insetBuilder.Configure(inset => inset.Content = content);
    }

    /// <summary>
    /// Declares a <see cref="GuiButton"/> slot at <paramref name="key"/>.
    /// Synchronous overload — accepts an <see cref="System.Action"/> for <paramref name="onClick"/>.
    /// For asynchronous handlers, use the <see cref="System.Func{T}"/>-returning-<see cref="System.Threading.Tasks.Task"/>
    /// overload below. The two overloads exist (rather than a single <see cref="GuiCallback"/>
    /// parameter) so plain lambdas like <c>() =&gt; DoStuff()</c> bind unambiguously without
    /// requiring an explicit cast — same DX as Blazor's overloaded callback APIs.
    /// </summary>
    public static IGuiTreeBuilder<GuiButton> AddButton(
        this IGuiTreeBuilder builder,
        int key,
        string text,
        Action? onClick = null)
        => AddButtonCore(builder, key, text,
            onClick is null ? default : (GuiCallback)onClick);

    /// <summary>
    /// Asynchronous-handler overload of <see cref="AddButton(IGuiTreeBuilder, int, string, System.Action)"/>.
    /// </summary>
    public static IGuiTreeBuilder<GuiButton> AddButton(
        this IGuiTreeBuilder builder,
        int key,
        string text,
        System.Func<System.Threading.Tasks.Task> onClick)
        => AddButtonCore(builder, key, text, onClick);

    private static IGuiTreeBuilder<GuiButton> AddButtonCore(
        IGuiTreeBuilder builder,
        int key,
        string text,
        GuiCallback onClick)
    {
        return builder.AddComponent<GuiButton>(key)
            .Configure(button =>
            {
                button.Text = text;
                button.OnClick = onClick;
            });
    }

    /// <summary>Sets <see cref="GuiButton.OnClick"/> to a synchronous handler. Method-group friendly.</summary>
    public static IGuiTreeBuilder<GuiButton> OnClick(this IGuiTreeBuilder<GuiButton> builder, System.Action handler)
        => builder.Configure(btn => btn.OnClick = handler);

    /// <summary>Sets <see cref="GuiButton.OnClick"/> to an asynchronous handler. Method-group friendly.</summary>
    public static IGuiTreeBuilder<GuiButton> OnClick(this IGuiTreeBuilder<GuiButton> builder, System.Func<System.Threading.Tasks.Task> handler)
        => builder.Configure(btn => btn.OnClick = handler);

    /// <summary>
    /// Opens a cascading value scope: <paramref name="value"/> is made available to every
    /// component slot declared anywhere inside <paramref name="content"/> (at any nesting
    /// depth) via <see cref="GuiComponent.GetCascadingValue{T}()"/> /
    /// <see cref="GuiSlot.TryGetCascadingValue{T}(out T)"/>.
    /// <para>
    /// Pass <paramref name="name"/> to distinguish multiple scopes of the same
    /// <typeparamref name="T"/> in the same ancestry; consumers must request the matching
    /// name. Inner scopes shadow outer scopes with the same <c>(Type, Name)</c> key.
    /// </para>
    /// <para>
    /// This is a purely logical operation — no component slot is created, nothing is added
    /// to the layout tree, and the scope closes automatically when <paramref name="content"/>
    /// returns.
    /// </para>
    /// </summary>
    public static void AddCascadingValue<T>(
        this IGuiTreeBuilder builder,
        T value,
        GuiTreeFragment content,
        string? name = null)
        => builder.PushCascadeScope(value, name, content);

    /// <summary>
    /// Declares a <see cref="GuiTooltip"/> wrapper at <paramref name="key"/>, attaching the
    /// floating <paramref name="tooltip"/> fragment to the regular layout child given by
    /// <paramref name="content"/>. The tooltip surfaces whenever the cursor hovers anywhere
    /// over the wrapped content's bounds. The tooltip is drawn on a separate Cairo surface
    /// managed by the dialog's <see cref="TooltipHost"/> (via a <c>FloatingLayerRenderer</c>), so it is free to extend beyond the
    /// wrapped content's parent bounds — and even beyond the dialog's surface — without
    /// clipping.
    /// <para>
    /// <b>Layout-transparent</b>: the wrapper itself does not occupy layout space and
    /// exposes no layout parameters. The slots declared inside <paramref name="content"/>
    /// flow at this declaration site exactly as if they had been added directly to
    /// <paramref name="builder"/>. The wrapper's hover region is derived from the union of
    /// those children's allocated bounds. Because <see cref="GuiTooltip"/> implements only
    /// <see cref="IGuiNode"/> (not <see cref="IGuiComponent"/>), it has no layout
    /// parameters of its own — set width/height/margin/padding etc. on <paramref name="content"/>'s
    /// inner components instead.
    /// </para>
    /// <para>
    /// The tooltip content is automatically wrapped in a <see cref="GuiTooltipBackground"/>
    /// panel painted with vanilla styling (DialogStrongBgColor fill, DialogBorderColor
    /// stroke, 5px content padding). Pass <paramref name="configureBackground"/> to retune
    /// any of those properties; for full chrome replacement, declare your own panel
    /// inside <paramref name="tooltip"/>.
    /// </para>
    /// <para>
    /// Requires a <see cref="TooltipHost"/> in scope — published automatically at the
    /// root of every <see cref="GuiDialog"/>. When the host is missing (e.g. a tooltip
    /// declared outside any dialog tree), the wrapped content still lays out normally
    /// but the tooltip never shows.
    /// </para>
    /// </summary>
    /// <returns>
    /// The fluent builder for the <see cref="GuiTooltip"/> slot, so callers can attach
    /// mouse handlers (e.g. <c>OnMouseEnter</c>) or chain further <c>Configure</c> calls.
    /// </returns>
    public static IGuiTreeBuilder<GuiTooltip> AddTooltip(
        this IGuiTreeBuilder builder,
        int key,
        GuiTreeFragment tooltip,
        GuiTreeFragment content,
        Action<GuiTooltipBackground>? configureBackground = null)
    {
        return builder.Add<GuiTooltip>(key).Configure(t =>
        {
            t.Content = content;
            t.TooltipContent = tooltip;
            t.ConfigureBackground = configureBackground;
        });
    }

    /// <summary>
    /// Declares a <see cref="GuiTextInput"/> slot at <paramref name="key"/>. Provides the
    /// initial <paramref name="text"/> and an <paramref name="onTextChanged"/> handler;
    /// pass <paramref name="mode"/> to restrict input to integer / decimal numbers, and
    /// optionally enable the right-edge spinner buttons via <paramref name="showSpinnerButtons"/>
    /// / <paramref name="spinnerInterval"/>.
    /// </summary>
    public static IGuiTreeBuilder<GuiTextInput> AddTextInput(
        this IGuiTreeBuilder builder,
        int key,
        string? text = null,
        Action<string>? onTextChanged = null,
        GuiTextInputMode? mode = null,
        string? placeholder = null,
        int? maxLength = null,
        GuiFontStyle? font = null,
        bool? showSpinnerButtons = null,
        double? spinnerInterval = null)
    {
        var inputBuilder = builder.AddComponent<GuiTextInput>(key);
        if (text is not null)
        {
            inputBuilder = inputBuilder.Configure(input => input.Text = text);
        }

        if (onTextChanged is not null)
        {
            inputBuilder = inputBuilder.Configure(input => input.OnTextChanged = onTextChanged);
        }

        if (mode is not null)
        {
            inputBuilder = inputBuilder.Configure(input => input.Mode = mode.Value);
        }

        if (placeholder is not null)
        {
            inputBuilder = inputBuilder.Configure(input => input.Placeholder = placeholder);
        }

        if (maxLength is not null)
        {
            inputBuilder = inputBuilder.Configure(input => input.MaxLength = maxLength.Value);
        }

        if (font is not null)
        {
            inputBuilder = inputBuilder.Configure(input => input.Font = font.Value);
        }

        if (showSpinnerButtons is not null)
        {
            inputBuilder = inputBuilder.Configure(input => input.ShowSpinnerButtons = showSpinnerButtons.Value);
        }

        if (spinnerInterval is not null)
        {
            inputBuilder = inputBuilder.Configure(input => input.SpinnerInterval = spinnerInterval.Value);
        }

        return inputBuilder;
    }

    /// <summary>
    /// Declares a numeric <see cref="GuiTextInput"/> slot at <paramref name="key"/> —
    /// shorthand for <see cref="AddTextInput"/> with <see cref="GuiTextInput.Mode"/>
    /// preset to <see cref="GuiTextInputMode.Decimal"/> (or <see cref="GuiTextInputMode.Integer"/>
    /// when <paramref name="integer"/> is true) and <see cref="GuiTextInput.ShowSpinnerButtons"/>
    /// enabled by default. Step size is configured via <paramref name="interval"/>
    /// (default <c>1</c>).
    /// </summary>
    public static IGuiTreeBuilder<GuiTextInput> AddNumberInput(
        this IGuiTreeBuilder builder,
        int key,
        string? text = null,
        Action<string>? onTextChanged = null,
        bool integer = false,
        double interval = 1,
        bool showSpinnerButtons = true,
        string? placeholder = null,
        int? maxLength = null,
        GuiFontStyle? font = null)
        => builder.AddTextInput(key,
            text: text,
            onTextChanged: onTextChanged,
            mode: integer ? GuiTextInputMode.Integer : GuiTextInputMode.Decimal,
            placeholder: placeholder,
            maxLength: maxLength,
            font: font,
            showSpinnerButtons: showSpinnerButtons,
            spinnerInterval: interval);

    /// <summary>
    /// Declares a <see cref="GuiCheckbox"/> slot at <paramref name="key"/>. Provides the
    /// initial <paramref name="checked_"/> state and an <paramref name="onCheckedChanged"/>
    /// handler.
    /// </summary>
    public static IGuiTreeBuilder<GuiCheckbox> AddCheckbox(
        this IGuiTreeBuilder builder,
        int key,
        bool? checked_ = null,
        Action<bool>? onCheckedChanged = null,
        double? size = null)
    {
        var checkboxBuilder = builder.AddComponent<GuiCheckbox>(key);
        if (checked_ is not null)
        {
            checkboxBuilder = checkboxBuilder.Configure(checkbox => checkbox.Checked = checked_.Value);
        }

        if (onCheckedChanged is not null)
        {
            checkboxBuilder = checkboxBuilder.Configure(checkbox => checkbox.OnCheckedChanged = onCheckedChanged);
        }

        if (size is not null)
        {
            checkboxBuilder = checkboxBuilder.Configure(checkbox => checkbox.Size = size.Value);
        }

        return checkboxBuilder;
    }

    /// <summary>
    /// Declares a <see cref="GuiSlider"/> slot at <paramref name="key"/>. Provides the
    /// initial <paramref name="value"/>, range / step / unit, and an
    /// <paramref name="onValueChanged"/> handler. Set <paramref name="triggerOnMouseUp"/>
    /// to defer the callback until the user releases the mouse — the visual still
    /// updates live during a drag, but the callback fires once with the final value.
    /// </summary>
    public static IGuiTreeBuilder<GuiSlider> AddSlider(
        this IGuiTreeBuilder builder,
        int key,
        int? value = null,
        int? minValue = null,
        int? maxValue = null,
        int? step = null,
        string? unit = null,
        Action<int>? onValueChanged = null,
        Func<int, string>? onTooltipText = null,
        bool? triggerOnMouseUp = null)
    {
        var sliderBuilder = builder.AddComponent<GuiSlider>(key);
        if (minValue is not null)
        {
            sliderBuilder = sliderBuilder.Configure(slider => slider.MinValue = minValue.Value);
        }

        if (maxValue is not null)
        {
            sliderBuilder = sliderBuilder.Configure(slider => slider.MaxValue = maxValue.Value);
        }

        if (step is not null)
        {
            sliderBuilder = sliderBuilder.Configure(slider => slider.Step = step.Value);
        }

        if (unit is not null)
        {
            sliderBuilder = sliderBuilder.Configure(slider => slider.Unit = unit);
        }

        if (value is not null)
        {
            sliderBuilder = sliderBuilder.Configure(slider => slider.Value = value.Value);
        }

        if (onValueChanged is not null)
        {
            sliderBuilder = sliderBuilder.Configure(slider => slider.OnValueChanged = onValueChanged);
        }

        if (onTooltipText is not null)
        {
            sliderBuilder = sliderBuilder.Configure(slider => slider.OnTooltipText = onTooltipText);
        }

        if (triggerOnMouseUp is not null)
        {
            sliderBuilder = sliderBuilder.Configure(slider => slider.TriggerOnMouseUp = triggerOnMouseUp.Value);
        }

        return sliderBuilder;
    }

    /// <summary>
    /// Declares a <see cref="GuiDropdown{T}"/> slot at <paramref name="key"/>. Provides
    /// the initial <paramref name="items"/> list, current <paramref name="selectedIndex"/>
    /// (or <c>-1</c> for "no selection"), and a selection callback. Pass
    /// <paramref name="itemTemplate"/> to render each row with a custom subtree (e.g. an
    /// icon + label) — by default the dropdown falls back to <c>item?.ToString()</c>
    /// rendered as a <see cref="GuiLabel"/>, which suits plain-string lists. Use a
    /// separate <paramref name="selectedTemplate"/> when the closed-state visual differs
    /// from the popup row (otherwise <paramref name="itemTemplate"/> is reused for both).
    /// </summary>
    public static IGuiTreeBuilder<GuiDropdown<T>> AddDropdown<T>(
        this IGuiTreeBuilder builder,
        int key,
        IReadOnlyList<T>? items = null,
        int? selectedIndex = null,
        Action<int>? onSelectionChanged = null,
        Action<T>? onItemSelected = null,
        GuiTreeFragment<T>? itemTemplate = null,
        GuiTreeFragment<T>? selectedTemplate = null,
        string? placeholder = null,
        GuiFontStyle? font = null,
        double? itemHeight = null,
        double? maxPopupHeight = null)
    {
        var dropdownBuilder = builder.AddComponent<GuiDropdown<T>>(key);
        if (items is not null)
        {
            dropdownBuilder = dropdownBuilder.Configure(dropdown => dropdown.Items = items);
        }

        if (selectedIndex is not null)
        {
            dropdownBuilder = dropdownBuilder.Configure(dropdown => dropdown.SelectedIndex = selectedIndex.Value);
        }

        if (onSelectionChanged is not null)
        {
            dropdownBuilder = dropdownBuilder.Configure(dropdown => dropdown.OnSelectionChanged = onSelectionChanged);
        }

        if (onItemSelected is not null)
        {
            dropdownBuilder = dropdownBuilder.Configure(dropdown => dropdown.OnItemSelected = onItemSelected);
        }

        if (itemTemplate is not null)
        {
            dropdownBuilder = dropdownBuilder.Configure(dropdown => dropdown.ItemTemplate = itemTemplate);
        }

        if (selectedTemplate is not null)
        {
            dropdownBuilder = dropdownBuilder.Configure(dropdown => dropdown.SelectedTemplate = selectedTemplate);
        }

        if (placeholder is not null)
        {
            dropdownBuilder = dropdownBuilder.Configure(dropdown => dropdown.Placeholder = placeholder);
        }

        if (font is not null)
        {
            dropdownBuilder = dropdownBuilder.Configure(dropdown => dropdown.Font = font.Value);
        }

        if (itemHeight is not null)
        {
            dropdownBuilder = dropdownBuilder.Configure(dropdown => dropdown.ItemHeight = itemHeight.Value);
        }

        if (maxPopupHeight is not null)
        {
            dropdownBuilder = dropdownBuilder.Configure(dropdown => dropdown.MaxPopupHeight = maxPopupHeight.Value);
        }

        return dropdownBuilder;
    }
}

