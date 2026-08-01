namespace BitzArt.VS.GUI;

public delegate void GuiTreeFragment(IGuiTreeBuilder builder);

public delegate void GuiTreeFragment<in T>(IGuiTreeBuilder builder, T item);
