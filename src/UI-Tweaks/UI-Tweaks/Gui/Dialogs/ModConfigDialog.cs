using BitzArt.UI.Tweaks.Config;
using BitzArt.VS.GUI;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace BitzArt.UI.Tweaks;

public class ModConfigDialog : VS.GUI.GuiDialog, IDisposable
{
    private const int SaveDebounceMs = 10000;
    private static readonly GuiLengthRule SidebarWidth = GuiLengthRule.Fraction(0.2);
    private const double SidebarSeparatorWidth = 1;
    private const double NavigationRowHeight = 44;

    private static readonly GuiColor SidebarPanelFillColor = GuiColor.FromRgba(0.08, 0.06, 0.04, 0.32);
    private static readonly GuiColor ContentPanelFillColor = GuiColor.FromRgba(0.15, 0.11, 0.08, 0.20);
    private static readonly GuiColor SidebarSeparatorColor = GuiColor.FromRgba(0, 0, 0, 0.34);
    private static readonly GuiColor BreadcrumbSeparatorColor = GuiColor.FromRgba(0.78, 0.69, 0.58, 0.10);

    private sealed record NavPage(string Label, GuiTreeFragment Content);

    private static readonly NavPage[] NavItems =
    [
        CreateNavPage<GeneralModConfigPage>(),
        CreateNavPage<ZoomModConfigPage>(),
        CreateNavPage<TooltipsModConfigPage>(),
    ];

    private static NavPage CreateNavPage<T>() where T : IModConfigPage, new()
        => new(T.PageName, builder => builder.Add<T>(0)
            .ConfigureLayout(layout => layout.Width = GuiLengthRule.Fill));

    private UiTweaksModConfig? _config;
    private ModConfigContext? _context;
    private Debouncer? _saveDebouncer;
    private ModConfigPageNavigator? _navigator;

    private ModConfigContext Context => _context
        ?? throw new InvalidOperationException($"{nameof(ModConfigDialog)} has not been configured.");

    private ModConfigPageNavigator Navigator => _navigator
        ?? throw new InvalidOperationException($"{nameof(ModConfigDialog)} has not been configured.");

    public ModConfigDialog()
    {
        IsResizable = true;
        MinWidth = 600;
        MinHeight = 360;
    }

    public void Configure(UiTweaksModConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (ReferenceEquals(_config, config) && _context is not null && _navigator is not null)
        {
            return;
        }

        _saveDebouncer?.Flush();
        _saveDebouncer?.Dispose();

        _config = config;
        _saveDebouncer = new Debouncer(
            TimeSpan.FromMilliseconds(SaveDebounceMs),
            () => ClientApi.StoreModConfig(config, Constants.ModConfigFileName));
        _context = new ModConfigContext(config, _saveDebouncer.Trigger);

        var initialPage = CreateNavPage<GeneralModConfigPage>();
        _navigator = new ModConfigPageNavigator(() => Slot!.RequestReconcile(), initialPage.Label, initialPage.Content);
    }

    public void Dispose()
    {
        _saveDebouncer?.Flush();
        _saveDebouncer?.Dispose();
        _saveDebouncer = null;

        GC.SuppressFinalize(this);
    }

    protected override void OnResizeUpdated(bool sizeChanged)
    {
        Slot!.RequestArrange();
    }

    protected override void ConfigureSlot(IGuiSlotBuilder builder)
    {
        base.ConfigureSlot(builder);
        builder.ConfigureLayout(layoutParameters =>
        {
            layoutParameters.Width = 650;
            layoutParameters.Height = 520;
            layoutParameters.Padding = new GuiThickness(0);
        });
    }

    protected override void BuildComponentTree(IGuiTreeBuilder builder)
    {
        builder.AddCascadingValue(Context, builder =>
        builder.AddCascadingValue(Navigator, builder =>
        {
            builder.Add<GuiContainer>(0)
                .Configure(container => container.Content = builder =>
                {
                    builder
                        .AddDialogTitleBar(0, Lang.Get($"{Constants.ModId}:ui-tweaks-config"),
                            onDrag: Move,
                            onClose: (GuiCallback)(Action)RequestClose);

                    builder
                        .AddDialogBackground(1, content: BuildBody)
                        .ConfigureLayout(layout =>
                        {
                            layout.Width = GuiLengthRule.Fill;
                            layout.Height = GuiLengthRule.Fill;
                        });
                })
                .ConfigureLayout(layout =>
                {
                    layout.Width = GuiLengthRule.Fill;
                    layout.Height = GuiLengthRule.Fill;
                });
        }));
    }

    private void BuildBody(IGuiTreeBuilder builder)
    {
        builder.Add<GuiContainer>(0)
            .Configure(container => container.Content = builder =>
            {
                builder.Add<GuiContainer>(0)
                    .Configure(container =>
                    {
                        container.Background = SidebarPanelFillColor;
                        container.Content = builder =>
                        {
                            for (int i = 0; i < NavItems.Length; i++)
                            {
                                int index = i;
                                var page = NavItems[index];
                                builder.Add<ConfigNavigationRow>(index)
                                    .ConfigureLayout(layout =>
                                    {
                                        layout.Height = NavigationRowHeight;
                                        layout.Width = GuiLengthRule.Fill;
                                    })
                                    .Configure(row =>
                                    {
                                        row.Text = page.Label;
                                        row.IsSelected = Navigator.RootPageName == page.Label;
                                        row.OnClick = (Action)(() => SelectPage(page));
                                    });
                            }
                        };
                    })
                    .ConfigureLayout(layout =>
                    {
                        layout.MinimumWidth = 200;
                        layout.Width = SidebarWidth;
                        layout.Height = GuiLengthRule.Fill;
                    });

                builder.AddRectangle(1, color: SidebarSeparatorColor)
                    .ConfigureLayout(layout =>
                    {
                        layout.Width = SidebarSeparatorWidth;
                        layout.Height = GuiLengthRule.Fill;
                    });

                builder.Add<GuiContainer>(2)
                    .Configure(container =>
                    {
                        container.Background = ContentPanelFillColor;
                        container.Content = builder =>
                        {
                            builder.Add<GuiContainer>(0)
                                .Configure(container => container.Content = builder =>
                                {
                                    builder.Add<GuiContainer>(0)
                                        .Configure(container => container.Content = builder =>
                                        {
                                            builder.Add<GuiBreadcrumbs>(0)
                                                .ConfigureLayout(layout => layout.Width = GuiLengthRule.Fill)
                                                .Configure(c =>
                                                {
                                                    c.CurrentItem = Navigator.CurrentPageName;
                                                    c.PreviousItems = Navigator.BreadcrumbPreviousItems;
                                                    c.OnItemClicked = name => Navigator.PopToName(name);
                                                });

                                            builder.AddRectangle(1, color: BreadcrumbSeparatorColor)
                                                .ConfigureLayout(layout =>
                                                {
                                                    layout.Height = 2;
                                                    layout.Width = GuiLengthRule.Fill;
                                                });
                                        })
                                        .ConfigureLayout(layout =>
                                        {
                                            layout.Width = GuiLengthRule.Fill;
                                            layout.Padding = new GuiThickness(Top: 14, Right: 10, Bottom: 8, Left: 10);
                                        });

                                    builder.Add<ConfigScrollPanel>(1)
                                        .Configure(container => container.Content = Navigator.CurrentContent)
                                        .ConfigureLayout(layout =>
                                        {
                                            layout.Width = GuiLengthRule.Fill;
                                            layout.Height = GuiLengthRule.Fill;
                                            layout.Margin = new GuiThickness(0, 8, 8, 8);
                                        });
                                })
                                .ConfigureLayout(layout =>
                                {
                                    layout.Width = GuiLengthRule.Fill;
                                    layout.Height = GuiLengthRule.Fill;
                                });
                        };
                    })
                    .ConfigureLayout(layout =>
                    {
                        layout.Width = GuiLengthRule.Fill;
                        layout.Height = GuiLengthRule.Fill;
                    });
            })
            .ConfigureLayout(layout =>
            {
                layout.Width = GuiLengthRule.Fill;
                layout.Height = GuiLengthRule.Fill;
            });
    }

    private void SelectPage(NavPage page)
    {
        if (Navigator.IsAtRoot(page.Label))
        {
            return;
        }

        Navigator.NavigateToRoot(page.Label, page.Content);
    }

}
