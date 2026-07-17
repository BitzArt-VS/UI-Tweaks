using BitzArt.UI.Tweaks.Gui;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Tests;

public class GuiComponentMeasurementTests
{
    [Fact]
    public void Measure_ComponentWithoutChildren_ShouldCollapse()
    {
        // Arrange
        var component = new TestContainer();
        Mount(component);

        // Act
        var measured = component.Measure(new GuiLayoutSize(100, 100));

        // Assert
        Assert.Equal(new GuiLayoutSize(0, 0), measured);
    }

    [Fact]
    public void Measure_VerticalRelativeChildren_ShouldStack()
    {
        // Arrange
        var root = new TestContainer();
        var first = new FixedMeasureComponent(10, 5);
        var second = new FixedMeasureComponent(7, 11);
        first.LayoutParameters.Margin = new GuiThickness(1, 2, 3, 4);
        second.LayoutParameters.Padding = new GuiThickness(vertical: 1, horizontal: 2);

        Mount(root,
            Slot(first),
            Slot(second));

        // Act
        var measured = root.Measure(new GuiLayoutSize(100, 100));

        // Assert
        Assert.Equal(new GuiLayoutSize(16, 22), measured);
    }

    [Fact]
    public void Measure_HorizontalRelativeChildren_ShouldStack()
    {
        // Arrange
        var root = new TestContainer();
        root.LayoutParameters.Direction = GuiDirection.Horizontal;

        Mount(root,
            Slot(new FixedMeasureComponent(10, 5)),
            Slot(new FixedMeasureComponent(7, 11)));

        // Act
        var measured = root.Measure(new GuiLayoutSize(100, 100));

        // Assert
        Assert.Equal(new GuiLayoutSize(17, 11), measured);
    }

    [Fact]
    public void Measure_TransparentNode_ShouldInlineChildren()
    {
        // Arrange
        var root = new TestContainer();

        Mount(root,
            Slot(new TransparentNode(),
                Slot(new FixedMeasureComponent(10, 5))));

        // Act
        var measured = root.Measure(new GuiLayoutSize(100, 100));

        // Assert
        Assert.Equal(new GuiLayoutSize(10, 5), measured);
    }

    [Fact]
    public void Measure_AbsoluteChild_ShouldSkipChild()
    {
        // Arrange
        var root = new TestContainer();
        var absolute = new FixedMeasureComponent(10, 5);
        absolute.LayoutParameters.Positioning = GuiComponentPositioning.Absolute;

        Mount(root, Slot(absolute));

        // Act
        var measured = root.Measure(new GuiLayoutSize(100, 100));

        // Assert
        Assert.Equal(new GuiLayoutSize(0, 0), measured);
    }

    [Fact]
    public void Measure_BoundedFillChild_ShouldNotMeasureChild()
    {
        // Arrange
        var root = new TestContainer();
        var child = new ThrowingMeasureComponent();
        child.LayoutParameters.WidthMode = GuiSizeMode.Fill;
        child.LayoutParameters.HeightMode = GuiSizeMode.Fill;

        Mount(root, Slot(child));

        // Act
        var measured = root.Measure(new GuiLayoutSize(100, 50));

        // Assert
        Assert.Equal(new GuiLayoutSize(100, 50), measured);
    }

    [Fact]
    public void Measure_UnboundedFillChild_ShouldUseContentMeasurement()
    {
        // Arrange
        var root = new TestContainer();
        var child = new TestContainer();
        child.LayoutParameters.WidthMode = GuiSizeMode.Fill;
        child.LayoutParameters.HeightMode = GuiSizeMode.Fill;

        Mount(root,
            Slot(child,
                Slot(new FixedMeasureComponent(12, 6))));

        // Act
        var measured = root.Measure(new GuiLayoutSize(double.PositiveInfinity, double.PositiveInfinity));

        // Assert
        Assert.Equal(new GuiLayoutSize(12, 6), measured);
    }

    [Fact]
    public void Measure_IntrinsicSizeOverride_ShouldCombineChildMeasurement()
    {
        // Arrange
        var component = new IntrinsicAndChildrenComponent(10, 4);
        Mount(component,
            Slot(new FixedMeasureComponent(25, 6)));

        // Act
        var measured = component.Measure(new GuiLayoutSize(100, 100));

        // Assert
        Assert.Equal(new GuiLayoutSize(25, 6), measured);
    }

    [Fact]
    public void MeasureContent_DefaultComponentMeasurement_ShouldMatch()
    {
        // Arrange
        var root = new TestContainer();
        root.LayoutParameters.Direction = GuiDirection.Horizontal;

        var childA = new FixedMeasureComponent(10, 5);
        childA.LayoutParameters.Margin = new GuiThickness(1, 2, 3, 4);

        var childB = new FixedMeasureComponent(7, 11);
        childB.LayoutParameters.Padding = new GuiThickness(vertical: 1, horizontal: 2);

        var transparent = new TransparentNode();

        Mount(root,
            Slot(childA),
            Slot(transparent,
                Slot(childB)));

        var available = new GuiLayoutSize(100, 100);

        // Act
        var measured = GuiComponentLayout.MeasureContent(
            root.RenderSlot.Children,
            available,
            root.LayoutParameters.Direction);
        var defaultMeasured = root.Measure(available);

        // Assert
        Assert.Equal(defaultMeasured, measured);
    }

    [Fact]
    public void Measure_DirectInterfaceImplementation_ShouldReusePublicHelpers()
    {
        // Arrange
        var root = new ExternalBaseComponent();

        Mount(root,
            Slot(new FixedMeasureComponent(12, 6)),
            Slot(new FixedMeasureComponent(8, 4)));

        // Act
        var measured = root.Measure(new GuiLayoutSize(100, 100));

        // Assert
        Assert.Equal(new GuiLayoutSize(12, 10), measured);
    }

    private static TestSlot Slot(IGuiNode node, params TestSlot[] children)
        => new(node, children);

    private static void Mount(IGuiNode node, params TestSlot[] children)
    {
        var rootSlot = Slot(node, children);
        rootSlot.AttachRecursive();
    }

    private sealed class TestContainer : GuiComponent
    {
        public IGuiNodeSlot RenderSlot => GetAttachedSlot(nameof(RenderSlot));
    }

    private sealed class TransparentNode : GuiNode;

    private sealed class ExternalBaseComponent : IGuiComponent
    {
        private IGuiNodeSlot? _slot;

        public GuiComponentLayoutParameters LayoutParameters { get; } = new();
        public GuiTreeFragment TreeFragment { get; } = _ => { };
        public IGuiNodeSlot RenderSlot => _slot!;

        public void Attach(IGuiNodeSlot slot)
            => _slot = slot;

        public GuiLayoutSize Measure(GuiLayoutSize available)
            => GuiComponentLayout.MeasureContent(
                _slot!.Children,
                available,
                LayoutParameters.Direction);
    }

    private sealed class FixedMeasureComponent(double width, double height) : GuiComponent
    {
        public override GuiLayoutSize Measure(GuiLayoutSize available)
            => new(width, height);
    }

    private sealed class ThrowingMeasureComponent : GuiComponent
    {
        public override GuiLayoutSize Measure(GuiLayoutSize available)
            => throw new InvalidOperationException("Measure should not be called for bounded fill sizing.");
    }

    private sealed class IntrinsicAndChildrenComponent(double width, double height) : GuiComponent
    {
        public override GuiLayoutSize Measure(GuiLayoutSize available)
        {
            var children = base.Measure(available);
            return new GuiLayoutSize(
                Math.Max(width, children.Width),
                Math.Max(height, children.Height));
        }
    }

    private sealed class TestSlot(IGuiNode node, IReadOnlyList<TestSlot> children) : IGuiNodeSlot
    {
        private readonly IReadOnlyList<IGuiNodeSlot> _children = children;

        public IGuiNode Node { get; } = node;
        public ICoreClientAPI ClientApi => null!;
        public IReadOnlyList<IGuiNodeSlot> Children => _children;

        public void AttachRecursive()
        {
            Node.Attach(this);

            for (int i = 0; i < children.Count; i++)
            {
                children[i].AttachRecursive();
            }
        }

        public void RequestReconcile() { }
        public void RequestLayout() { }
        public void RequestRender() { }

        public bool TryGetCascadingValue<T>(out T value)
            => TryGetCascadingValue(name: null, out value);

        public bool TryGetCascadingValue<T>(string? name, out T value)
        {
            value = default!;
            return false;
        }
    }
}
